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

        // SEARCH
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            query = query.Where(p =>
                p.Name.Contains(filter.Search) ||
                p.Description.Contains(filter.Search));
        }

        // CATEGORY FILTER
        if (!string.IsNullOrWhiteSpace(filter.Category))
            query = query.Where(p => p.Category == filter.Category);

        // SUPPLIER FILTER
        if (filter.SupplierId.HasValue)
            query = query.Where(p => p.SupplierId == filter.SupplierId);

        // AVAILABILITY
        if (filter.IsAvailable.HasValue)
        {
            if (filter.IsAvailable.Value)
                query = query.Where(p => p.Quantity > 0);
            else
                query = query.Where(p => p.Quantity == 0);
        }

            // PRICE RANGE
            if (filter.MinPrice.HasValue)
                query = query.Where(p => p.Price >= filter.MinPrice.Value);

            if (filter.MaxPrice.HasValue)
                query = query.Where(p => p.Price <= filter.MaxPrice.Value);

            // SORTING
                if (!string.IsNullOrWhiteSpace(filter.SortBy))
                {
                    switch (filter.SortBy.ToLower())
                    {
                        case "price":
                            query = filter.Desc ? query.OrderByDescending(p => p.Price)
                                                : query.OrderBy(p => p.Price);
                            break;

                        case "name":
                            query = filter.Desc ? query.OrderByDescending(p => p.Name)
                                                : query.OrderBy(p => p.Name);
                            break;
                    }
                }

                var products = await query.ToListAsync();

                // MAP USING AUTOMAPPER
                var result = _mapper.Map<List<ProductDto>>(products);

            return Ok(result);

            }

        }
    }
