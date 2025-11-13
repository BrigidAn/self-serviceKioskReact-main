using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KioskAPI.Data;
using KioskAPI.Models;
using KioskAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KioskAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly AppDbContext _context;
         public AccountController(AppDbContext context) => _context = context;

   
        // Returns the current user's balance
        [HttpGet("balance")]
        public async Task<IActionResult> GetBalance()
        {
            var userId = GetUserId();
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.UserId == userId);
            if (account == null) return NotFound(new { message = "Account not found" });

            return Ok(new { balance = account.Balance });
        }

      
        // Top up the current user's account by a specified amount
        [HttpPost("topup")]
        public async Task<IActionResult> TopUp([FromBody] decimal amount)
        {
            if (amount <= 0) return BadRequest(new { message = "Amount must be positive" });

            var userId = GetUserId();
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.UserId == userId);
            if (account == null) return NotFound(new { message = "Account not found" });

            // Create transaction
            var tx = new Transaction
            {
                AccountId = account.AccountId,
                Type = "credit",
                TotalAmount = amount,
                Description = "Top-up",
                CreatedAt = DateTime.UtcNow
            };

            account.Balance += amount;
            account.LastUpdated = DateTime.UtcNow;
            _context.Transactions.Add(tx);
            await _context.SaveChangesAsync();

            return Ok(new { balance = account.Balance });
        }

        // Helper to read user ID from JWT claim "userId"
        private int GetUserId()
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            return int.TryParse(userIdClaim, out var id) ? id : 0;
        }
    }
}