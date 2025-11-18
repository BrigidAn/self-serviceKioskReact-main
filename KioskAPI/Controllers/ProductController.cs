using System.Linq;
using System.Threading.Tasks;
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

     [HttpPost("filter")]
        public async Task<IActionResult> FilterProducts([FromBody] ProductFilterDto filter)
        {
            var query = _context.Products
                .Include(p => p.Supplier)
                .AsQueryable();

            // 🔍 SEARCH (NAME + DESCRIPTION + CATEGORY SO "laptops" SHOWS ONLY LAPTOPS)
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                string search = filter.Search.ToLower();

                query = query.Where(p =>
                    p.Name.ToLower().Contains(search) ||
                    p.Description!.ToLower().Contains(search) ||
                    p.Category.ToLower().Contains(search)
                );
            }

            // CATEGORY FILTER
            if (!string.IsNullOrWhiteSpace(filter.Category))
                query = query.Where(p => p.Category.ToLower() == filter.Category.ToLower());

            // SUPPLIER FILTER
            if (filter.SupplierId.HasValue)
                query = query.Where(p => p.SupplierId == filter.SupplierId);

            // AVAILABILITY
            if (filter.IsAvailable.HasValue)
            {
                query = filter.IsAvailable.Value
                    ? query.Where(p => p.Quantity > 0)
                    : query.Where(p => p.Quantity == 0);
            }

            // PRICE RANGE
            if (filter.MinPrice.HasValue)
                query = query.Where(p => p.Price >= filter.MinPrice.Value);

            if (filter.MaxPrice.HasValue)
                query = query.Where(p => p.Price <= filter.MaxPrice.Value);

            // SORTING
            query = filter.SortBy?.ToLower() switch
            {
                "price" => filter.Desc ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
                "name"  => filter.Desc ? query.OrderByDescending(p => p.Name)  : query.OrderBy(p => p.Name),
                _       => query.OrderBy(p => p.ProductId)
            };

            // PAGINATION
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)filter.PageSize);

            var products = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var result = _mapper.Map<List<ProductDto>>(products);

            return Ok(new
            {
                TotalItems = totalItems,
                TotalPages = totalPages,
                Page = filter.Page,
                PageSize = filter.PageSize,
                Data = result
            });
        }
    }
}