using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KioskAPI.Data;
using KioskAPI.Dtos;
using KioskAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KioskAPI.Controllers
{
     [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        // 🧍‍♂️ Get user profile
        [HttpGet("profile/{userId}")]
        public async Task<IActionResult> GetUserProfile(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Role).FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return NotFound(new { message = "User not found" });

            return Ok(new
            {
                user.UserId,
                user.Name,
                user.Email,
                Role = user.Role?.RoleName,
                user.CreatedAt
            });
        }

        // 💰 Get account details
        [HttpGet("account/{userId}")]
        public async Task<IActionResult> GetAccountDetails(int userId)
        {
            var account = await _context.Accounts
                .Include(a => a.Transactions)
                .FirstOrDefaultAsync(a => a.UserId == userId);

            if (account == null)
                return NotFound(new { message = "Account not found" });

            return Ok(new
            {
                account.AccountId,
                account.Balance,
                account.LastUpdated,
                Transactions = account.Transactions?.Select(t => new
                {
                    t.TransactionId,
                    t.Type,
                    t.TotalAmount,
                    t.Description,
                    t.CreatedAt
                })
            });
        }

        // Add funds to account
       [HttpPost("account/topup")]
        public async Task<IActionResult> TopUpBalance([FromBody] TopUpDto data)
        {
            // 1. Basic validation
            if (data.Amount <= 0)
                return BadRequest(new { message = "Amount must be greater than zero." });

            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.UserId == data.UserId);

            // 2. Account not found
            if (account == null)
                return NotFound(new { message = "Account not found." });

            // 4. Optional: Maximum balance (example: R100,000)
            if (account.Balance + data.Amount > 100000)
                return BadRequest(new { message = "Balance cannot exceed R100,000." });

            // Apply the top-up
            account.Balance += data.Amount;
            account.LastUpdated = DateTime.UtcNow;

            var transaction = new Transaction
            {
                AccountId = account.AccountId,
                Type = "credit",
                TotalAmount = data.Amount,
                Description = "Balance top-up",
                CreatedAt = DateTime.UtcNow
            };

            _context.Transactions.Add(transaction);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Balance topped up successfully",
                balance = account.Balance
            });
        }


        // 🧾 Get user orders
        [HttpGet("orders/{userId}")]
        public async Task<IActionResult> GetUserOrders(int userId)
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.UserId == userId)
                .Select(o => new
                {
                    o.OrderId,
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

            if (!orders.Any())
                return NotFound(new { message = "No orders found for this user" });

            return Ok(orders);
        }


        [HttpGet("account/{userId}/transactions")]
        public async Task<IActionResult> GetTransactionHistory(int userId)
        {
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.UserId == userId);

            if (account == null)
                return NotFound(new { message = "Account not found." });

            var transactions = await _context.Transactions
                .Where(t => t.AccountId == account.AccountId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return Ok(transactions);
        }

    }
}