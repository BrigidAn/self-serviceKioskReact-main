using System.Linq;
using System.Threading.Tasks;
using KioskAPI.Data;
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

        public ProductController(AppDbContext context)
        {
            _context = context;
        }

        // GET ALL PRODUCTS
        // Returns clean JSON + includes ImageUrl + Available flag
        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _context.Products
                .Include(p => p.Supplier)
                .Select(p => new
                {
                    productId = p.ProductId,
                    name = p.Name,
                    description = p.Description,
                    price = p.Price,
                    category = p.Category,
                    quantity = p.Quantity,
                    imageUrl = p.ImageUrl,
                    supplier = p.Supplier,
                    

                    // EASY indicator for React (no quantity = unavailable)
                    isAvailable = p.Quantity > 0
                })
                .ToListAsync();

            return Ok(products);
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

            return Ok(new
            {
                productId = product.ProductId,
                name = product.Name,
                description = product.Description,
                price = product.Price,
                category = product.Category,
                quantity = product.Quantity,
                imageUrl = product.ImageUrl,
                supplier = product.Supplier != null ? product.Supplier.Name : "Unknown",
                isAvailable = product.Quantity > 0
            });
        }

        // ADD NEW PRODUCT (ADMIN)
        [HttpPost]
        public async Task<IActionResult> AddProduct([FromBody] Product newProduct)
        {
            if (newProduct.Quantity < 0)
                return BadRequest(new { message = "Quantity cannot be negative" });

            _context.Products.Add(newProduct);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Product added successfully",
                productId = newProduct.ProductId
            });
        }

        // UPDATE PRODUCT (ADMIN)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] Product updatedProduct)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
                return NotFound(new { message = "Product not found" });

            if (updatedProduct.Quantity < 0)
                return BadRequest(new { message = "Quantity cannot be negative" });

            product.Name = updatedProduct.Name;
            product.Description = updatedProduct.Description;
            product.Price = updatedProduct.Price;
            product.Category = updatedProduct.Category;
            product.ImageUrl = updatedProduct.ImageUrl;
            product.Quantity = updatedProduct.Quantity;
            product.SupplierId = updatedProduct.SupplierId;

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
    }
}
