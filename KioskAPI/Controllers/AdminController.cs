using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KioskAPI.Data;
using KioskAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KioskAPI.Controllers
{
   [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ Check if user is admin before accessing data
        private async Task<bool> IsAdminAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            return user?.Role?.RoleName == "Admin";
        }

        // 🧑‍💻 Get all users
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers([FromQuery] int adminId)
        {
            if (!await IsAdminAsync(adminId))
                return Unauthorized(new { message = "Access denied. Admins only." });

            var users = await _context.Users
                .Include(u => u.Role)
                .Select(u => new
                {
                    u.UserId,
                    u.Name,
                    u.Email,
                    Role = u.Role.RoleName,
                    u.CreatedAt,
                    u.UpdatedAt
                })
                .ToListAsync();

            return Ok(users);
        }

        // 💰 Get all transactions
        [HttpGet("transactions")]
        public async Task<IActionResult> GetAllTransactions([FromQuery] int adminId)
        {
            if (!await IsAdminAsync(adminId))
                return Unauthorized(new { message = "Access denied. Admins only." });

            var transactions = await _context.Transactions
                .Include(t => t.Account)
                .ThenInclude(a => a.User)
                .Select(t => new
                {
                    t.TransactionId,
                    AccountOwner = t.Account.User.Name,
                    t.Type,
                    t.TotalAmount,
                    t.Description,
                    t.CreatedAt
                })
                .ToListAsync();

            return Ok(transactions);
        }

        // 🧾 Get all orders
        [HttpGet("orders")]
        public async Task<IActionResult> GetAllOrders([FromQuery] int adminId)
        {
            if (!await IsAdminAsync(adminId))
                return Unauthorized(new { message = "Access denied. Admins only." });

            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .Select(o => new
                {
                    o.OrderId,
                    Customer = o.User.Name,
                    o.OrderDate,
                    o.TotalAmount,
                    o.Status,
                    o.PaymentStatus,
                    Items = o.OrderItems.Select(i => new
                    {
                        i.ProductId,
                        i.Quantity,
                        i.PriceAtPurchase
                    })
                })
                .ToListAsync();

            return Ok(orders);
        }

        // 📦 Get all products
        [HttpGet("products")]
        public async Task<IActionResult> GetAllProducts([FromQuery] int adminId)
        {
            if (!await IsAdminAsync(adminId))
                return Unauthorized(new { message = "Access denied. Admins only." });

            var products = await _context.Products
                .Include(p => p.Supplier)
                .Select(p => new
                {
                    p.ProductId,
                    p.Name,
                    p.Description,
                    p.Price,
                    p.Category,
                    p.Quantity,
                    Supplier = p.Supplier.Name
                })
                .ToListAsync();

            return Ok(products);
        }

        [HttpPost("add-product")] //adding new products 
        public async Task<IActionResult> AddProduct([FromQuery] int adminId, [FromBody] Product newProduct)
        {
            if (!await IsAdminAsync(adminId))
                return Unauthorized(new { message = "Access denied. Admins only." });

            _context.Products.Add(newProduct);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Product added successfully.", ProductId = newProduct.ProductId });
        }
    }
}