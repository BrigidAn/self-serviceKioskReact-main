namespace KioskAPI.Controllers
{
  using System.Linq;
  using System.Threading.Tasks;
  using KioskAPI.Data;
  using KioskAPI.Dtos;
  using KioskAPI.Mappers;
  using KioskAPI.Services;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.EntityFrameworkCore;

  /// <summary>
  /// Manages products available in the self-service kiosk system.
  /// Supports browsing, filtering, and full CRUD operations for administrators.
  /// </summary>
  [ApiController]
  [Route("api/[controller]")]
  public class ProductController : ControllerBase
  {
    private readonly AppDbContext _context;
    private readonly CloudinaryService _cloudinary;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductController"/>.
    /// </summary>
    /// <param name="context">Database context.</param>
    /// <param name="cloudinary">Cloudinary service for image uploads.</param>
    public ProductController(AppDbContext context, CloudinaryService cloudinary)
    {
      this._context = context;
      this._cloudinary = cloudinary;
    }

    /// <summary>
    /// Retrieves all products including their supplier information.
    /// Publicly accessible endpoint for product listing.
    /// </summary>
    /// <returns>
    /// 200 OK with a list of products.
    /// </returns>
    // KioskAPI/Controllers/ProductController.cs
    [HttpGet]
    public async Task<IActionResult> GetProducts([FromQuery] bool adminView = false)
    {
      var query = this._context.Products.Include(p => p.Supplier).AsQueryable();

      if (!adminView) // normal users
      {
        query = query.Where(p => p.IsAvailable);
      }

      var products = await query.ToListAsync().ConfigureAwait(true);
      var productDtos = products.Select(ProductMapper.ToDto).ToList();

      return this.Ok(productDtos);
    }

    /// <summary>
    /// Retrieves a single product by its identifier.
    /// </summary>
    /// <param name="id">The product identifier.</param>
    /// <returns>
    /// 200 OK with the product,
    /// 404 Not Found if the product does not exist.
    /// </returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(int id)
    {
      if (!this.ModelState.IsValid)
      {
        return this.BadRequest(this.ModelState);
      }

      var product = await this._context.Products
          .Include(p => p.Supplier)
          .FirstOrDefaultAsync(p => p.ProductId == id).ConfigureAwait(true);

      if (product == null)
      {
        return this.NotFound(new { message = "Product not found" });
      }

      var dto = ProductMapper.ToDto(product);
      return this.Ok(dto);
    }

    /// <summary>
    /// Creates a new product with an uploaded image.
    /// Admin-only endpoint.
    /// </summary>
    /// <param name="dto">Product creation data including image file.</param>
    /// <returns>
    /// 200 OK if the product is created successfully,
    /// 400 Bad Request for validation or duplication errors.
    /// </returns>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddProduct([FromForm] CreateProductDto dto)
    {
      if (!this.ModelState.IsValid)
      {
        return this.BadRequest(this.ModelState);
      }

      var imageUrl = await this._cloudinary.UploadImageAsync(dto.File).ConfigureAwait(true);
      dto.ImageUrl = imageUrl;

      if (string.IsNullOrWhiteSpace(dto.ImageUrl))
      {
        return this.BadRequest("Image URL is required for a product.");
      }

      var existingProduct = await this._context.Products
      .FirstOrDefaultAsync(p => p.Name.ToLower() == dto.Name.ToLower()
      && p.Category.ToLower() == dto.Category.ToLower() && p.ImageUrl.ToLower() == dto.ImageUrl.ToLower()).ConfigureAwait(true);

      if (existingProduct != null)
      {
        return this.BadRequest(new { message = "Product already exists" });
      }

      var product = ProductMapper.ToEntity(dto);
      this._context.Products.Add(product);
      await this._context.SaveChangesAsync().ConfigureAwait(true);

      return this.Ok(new
      {
        message = "Product added successfully",
        productId = product.ProductId
      });
    }

    /// <summary>
    /// Updates an existing product’s details.
    /// Allows optional image replacement via Cloudinary.
    /// Admin-only endpoint.
    /// </summary>
    /// <param name="id">The product identifier.</param>
    /// <param name="dto">Updated product data.</param>
    /// <returns>
    /// 200 OK if update succeeds,
    /// 404 Not Found if product does not exist.
    /// </returns>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateProduct(int id, [FromForm] UpdateProductDto dto)
    {
      if (!this.ModelState.IsValid)
      {
        return this.BadRequest(this.ModelState);
      }

      var product = await this._context.Products
          .FirstOrDefaultAsync(p => p.ProductId == id).ConfigureAwait(true);

      if (product == null)
      {
        return this.NotFound(new { message = "Product not found" });
      }

      if (dto.Quantity < 0)
      {
        return this.BadRequest(new { message = "Quantity cannot be negative" });
      }

      if (dto.File != null)
      {
        var newImageUrl = await this._cloudinary.UploadImageAsync(dto.File).ConfigureAwait(true);
        if (!string.IsNullOrWhiteSpace(newImageUrl))
        {
          dto.ImageUrl = newImageUrl;
        }
      }

      ProductMapper.UpdateEntity(product, dto);

      await this._context.SaveChangesAsync().ConfigureAwait(true);
      return this.Ok(new { message = "Product updated successfully" });
    }

    /// <summary>
    /// Deletes a product from the system.
    /// Admin-only endpoint.
    /// </summary>
    /// <param name="id">The product identifier.</param>
    /// <returns>
    /// 200 OK if deleted,
    /// 404 Not Found if product does not exist.
    /// </returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
      var product = await this._context.Products.FindAsync(id).ConfigureAwait(true);

      if (product == null)
      {
        return this.NotFound(new { message = "Product not found" });
      }

      this._context.Products.Remove(product);
      await this._context.SaveChangesAsync().ConfigureAwait(true);

      return this.Ok(new { message = "Product deleted successfully" });
    }

    /// <summary>
    /// Retrieves products using advanced filtering, searching, and sorting options.
    /// Used by kiosk UI for browsing and category filtering.
    /// </summary>
    /// <param name="search">Text search across name, description, and category.</param>
    /// <param name="category">Exact category match.</param>
    /// <param name="isAvailable">Filters by stock availability.</param>
    /// <param name="minPrice">Minimum price filter.</param>
    /// <param name="maxPrice">Maximum price filter.</param>
    /// <param name="sortBy">Sort field (price or name).</param>
    /// <param name="desc">Sort descending if true.</param>
    /// <returns>
    /// 200 OK with filtered products and available categories.
    /// </returns>
    [HttpGet("FilterProducts")]
    public async Task<IActionResult> GetProducts(
        [FromQuery] string? search,
        [FromQuery] string? category,
        [FromQuery] bool? isAvailable,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] string? sortBy,
        [FromQuery] bool desc = false)
    {
      var query = this._context.Products.Include(p => p.Supplier).AsQueryable();

      if (!string.IsNullOrWhiteSpace(search))
      {
        string s = search.ToLower();
        query = query.Where(p =>
            (p.Name ?? "").ToLower().Contains(s) ||
            (p.Description ?? "").ToLower().Contains(s) ||
            (p.Category ?? "").ToLower().Contains(s)
        );
      }

      if (!string.IsNullOrWhiteSpace(category))
      {
        query = query.Where(p => (p.Category ?? "").ToLower() == category.ToLower());
      }

      if (isAvailable.HasValue)
      {
        query = isAvailable.Value
            ? query.Where(p => p.Quantity > 0)
            : query.Where(p => p.Quantity == 0);
      }

      if (minPrice.HasValue)
      {
        query = query.Where(p => p.Price >= minPrice.Value);
      }

      if (maxPrice.HasValue)
      {
        query = query.Where(p => p.Price <= maxPrice.Value);
      }

      if (!string.IsNullOrWhiteSpace(sortBy))
      {
        query = sortBy.ToLower() switch
        {
          "price" => desc ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
          "name" => desc ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
          _ => query
        };
      }

      var products = await query.ToListAsync().ConfigureAwait(true);
      var productDtos = products.Select(ProductMapper.ToDto).ToList();

      var categories = await this._context.Products
          .Select(p => p.Category)
          .Distinct()
          .ToListAsync().ConfigureAwait(true);

      return this.Ok(new
      {
        Products = productDtos,
        Categories = categories
      });
    }

    /// <summary>
    /// Updates the availability status of a product.
    /// Admin-only endpoint.
    /// </summary>
    /// <param name="id">The product identifier.</param>
    /// <param name="dto">Availability update data.</param>
    /// <returns>
    /// 200 OK if updated,
    /// 404 Not Found if product does not exist.
    /// </returns>
    [HttpPatch("{id}/availability")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateAvailability(int id, [FromBody] UpdateProductAvailabilityDto dto)
    {
      if (!this.ModelState.IsValid)
      {
        return this.BadRequest(this.ModelState);
      }

      var product = await this._context.Products.FindAsync(id).ConfigureAwait(true);
      if (product == null)
      {
        return this.NotFound(new { message = "Product not found" });
      }

      product.IsAvailable = dto.IsAvailable;

      await this._context.SaveChangesAsync().ConfigureAwait(true);
      return this.Ok(new { message = "Product availability updated" });
    }

    [HttpPut("{id}/availability")]
    public async Task<IActionResult> ToggleAvailability(int id, [FromBody] UpdateProductAvailabilityDto dto)
    {
      var product = await this._context.Products.FindAsync(id).ConfigureAwait(true);
      if (product == null)
      {
        return this.NotFound();
      }

      product.IsAvailable = dto.IsAvailable;
      await this._context.SaveChangesAsync().ConfigureAwait(true);

      return this.Ok(new { product.ProductId, product.IsAvailable });
    }

  }
}