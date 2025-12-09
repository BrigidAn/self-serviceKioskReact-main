namespace KioskAPI.Controllers
{
  using System.Linq;
  using System.Threading.Tasks;
  using KioskAPI.Data;
  using KioskAPI.Dtos;
  using KioskAPI.Mappers;
  using KioskAPI.Models;
  using KioskAPI.Services;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.EntityFrameworkCore;

  [ApiController]
  [Route("api/[controller]")]
  public class ProductController : ControllerBase
  {
    private readonly AppDbContext _context;
    private readonly CloudinaryService _cloudinary;
    public ProductController(AppDbContext context, CloudinaryService cloudinary)
    {
      this._context = context;
      this._cloudinary = cloudinary;
    }

    // GET ALL PRODUCTS
    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
      var products = await this._context.Products
          .Include(p => p.Supplier)
          .ToListAsync().ConfigureAwait(true);

      var productDtos = products.Select(ProductMapper.ToDto).ToList();
      return this.Ok(productDtos);
    }

    // GET PRODUCT BY ID
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

    // ADD NEW PRODUCT (ADMIN)
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddProduct([FromForm] CreateProductDto dto)
    {
      if (!this.ModelState.IsValid)
      {
        return this.BadRequest(this.ModelState);
      }

      var imageUrl = await _cloudinary.UploadImageAsync(dto.File).ConfigureAwait(true);
      dto.ImageUrl = imageUrl;

      if (string.IsNullOrWhiteSpace(dto.ImageUrl))
      {
        return this.BadRequest("Image URL is required for a product.");
      }
      //this is to view existing products dontforget
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

    // UPDATE PRODUCT (ADMIN)
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

      ProductMapper.UpdateEntity(product, dto);  // Only updates non-null fields

      await this._context.SaveChangesAsync().ConfigureAwait(true);
      return this.Ok(new { message = "Product updated successfully" });
    }

    // DELETE PRODUCT (ADMIN)
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
      // Start query from products including supplier
      var query = this._context.Products.Include(p => p.Supplier).AsQueryable();

      // SEARCH: name, description, category
      if (!string.IsNullOrWhiteSpace(search))
      {
        string s = search.ToLower();
        query = query.Where(p =>
            (p.Name ?? "").ToLower().Contains(s) ||
            (p.Description ?? "").ToLower().Contains(s) ||
            (p.Category ?? "").ToLower().Contains(s)
        );
      }

      // CATEGORY FILTER
      if (!string.IsNullOrWhiteSpace(category))
      {
        query = query.Where(p => (p.Category ?? "").ToLower() == category.ToLower());
      }

      // AVAILABILITY
      if (isAvailable.HasValue)
      {
        query = isAvailable.Value
            ? query.Where(p => p.Quantity > 0)
            : query.Where(p => p.Quantity == 0);
      }

      // PRICE RANGE
      if (minPrice.HasValue)
      {
        query = query.Where(p => p.Price >= minPrice.Value);
      }

      if (maxPrice.HasValue)
      {
        query = query.Where(p => p.Price <= maxPrice.Value);
      }

      // SORTING
      if (!string.IsNullOrWhiteSpace(sortBy))
      {
        query = sortBy.ToLower() switch
        {
          "price" => desc ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
          "name" => desc ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
          _ => query
        };
      }

      // Fetch products
      var products = await query.ToListAsync().ConfigureAwait(true);
      var productDtos = products.Select(ProductMapper.ToDto).ToList();

      // Fetch distinct categories
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
  }
}