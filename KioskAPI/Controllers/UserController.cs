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
    //[Authorize(Roles = "User")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        public UserController(AppDbContext context) => _context = context;
       

    // GET: api/admin/users
        // Admin can view all users and their roles
        [HttpGet("{UserId}")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users.Include(u => u.Role)
                .Select(u => new {
                    u.UserId,
                    u.Name,
                    u.Email,
                    Role = u.Role != null ? u.Role.RoleName : null,
                    u.CreatedAt
                }).ToListAsync();

            return Ok(users);
        }

        // GET: api/admin/accounts
        // Admin can view all account balances and last updated time
        [HttpGet("accounts")]
        public async Task<IActionResult> GetAllAccounts()
        {
            var accounts = await _context.Accounts
                .Include(a => a.User)
                .Select(a => new {
                    a.AccountId,
                    User = a.User != null ? a.User.Email : null,
                    a.Balance,
                    a.LastUpdated
                }).ToListAsync();

            return Ok(accounts);
        }

        // POST: api/admin/credit/{userId}
        // Admin can credit a user's account (for adjustments)
        [HttpPost("credit/{userId}")]
        public async Task<IActionResult> CreditUser(int userId, [FromBody] decimal amount)
        {
            if (amount <= 0) return BadRequest(new { message = "Amount must be positive" });

            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.UserId == userId);
            if (account == null) return NotFound(new { message = "Account not found" });

            var tx = new Transaction
            {
                AccountId = account.AccountId,
                Type = "credit",
                TotalAmount = amount,
                Description = "Admin credit"
            };

            account.Balance += amount;
            account.LastUpdated = DateTime.UtcNow;

            _context.Transactions.Add(tx);
            await _context.SaveChangesAsync();

            return Ok(new { balance = account.Balance });
        }
    }
}