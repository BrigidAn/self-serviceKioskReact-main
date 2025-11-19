using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using KioskAPI.Data;
using KioskAPI.Dtos;
using KioskAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KioskAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public ProductController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET ALL PRODUCTS
        [HttpGet]
         public async Task<IActionResult> GetProducts()
        {
            var products = await _context.Products
                .Include(p => p.Supplier)
                .ToListAsync();

            var productDtos = _mapper.Map<List<ProductDto>>(products);
            return Ok(productDtos);
        }

        // GET PRODUCT BY ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.Supplier)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
                return NotFound(new { message = "Product not found" });

            var dto = _mapper.Map<ProductDto>(product);
            return Ok(dto);
        }

        // ADD NEW PRODUCT (ADMIN)
        [HttpPost]
        public async Task<IActionResult> AddProduct([FromBody] CreateProductDto dto)
        {
            if (dto.Quantity < 0)
                return BadRequest(new { message = "Quantity cannot be negative" });

            var product = _mapper.Map<Product>(dto);
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Product added successfully",
                productId = product.ProductId
            });
        }

        // UPDATE PRODUCT (ADMIN)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto dto)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
                return NotFound(new { message = "Product not found" });

            if (dto.Quantity < 0)
                return BadRequest(new { message = "Quantity cannot be negative" });

            _mapper.Map(dto, product);  // Only updates non-null fields

            await _context.SaveChangesAsync();
            return Ok(new { message = "Product updated successfully" });
        }

        // DELETE PRODUCT (ADMIN)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
                return NotFound(new { message = "Product not found" });

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Product deleted successfully" });
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
            var query = _context.Products.Include(p => p.Supplier).AsQueryable();

            // SEARCH: name, description, category
            if (!string.IsNullOrWhiteSpace(search))
            {
                string s = search.ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(s) ||
                    p.Description.ToLower().Contains(s) ||
                    p.Category.ToLower().Contains(s)
                );
            }

            // CATEGORY FILTER
            if (!string.IsNullOrWhiteSpace(category))
                query = query.Where(p => p.Category.ToLower() == category.ToLower());

            // AVAILABILITY
            if (isAvailable.HasValue)
                query = isAvailable.Value
                    ? query.Where(p => p.Quantity > 0)
                    : query.Where(p => p.Quantity == 0);

            // PRICE RANGE
            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);

            // SORTING
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                query = sortBy.ToLower() switch
                {
                    "price" => desc ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
                    "name"  => desc ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
                    _ => query
                };
            }

            // Fetch products
            var products = await query.ToListAsync();
            var productDtos = _mapper.Map<List<ProductDto>>(products);

            // Fetch distinct categories
            var categories = await _context.Products
                .Select(p => p.Category)
                .Distinct()
                .ToListAsync();

            return Ok(new
            {
                Products = productDtos,
                Categories = categories
            });
        }
    }
}
