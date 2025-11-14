using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KioskAPI.Data;
using KioskAPI.Dtos;
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
            if (userId == 0) return Unauthorized(new { message = "Not logged In" });

            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.UserId == userId);
            if (account == null) return NotFound(new { message = "Account not found" });

            return Ok(new { balance = account.Balance });
        }

      
        // Top up the current user's account by a specified amount
       [HttpPost("topup")]
        public async Task<IActionResult> TopUp([FromBody] TopUpDto dto)
        {
            if (dto.Amount <= 0) 
            return BadRequest(new { message = "Amount must be positive" });

            var userId = GetUserId();
            if (userId == 0) return Unauthorized(new { message = "Not logged In" });

            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.UserId == userId);
            if (account == null) return NotFound(new { message = "Account not found" });

            var tx = new Transaction
            {
                AccountId = account.AccountId,
                Type = "credit",
                TotalAmount = dto.Amount,
                Description = "Top-up",
                CreatedAt = DateTime.UtcNow
            };

            account.Balance += dto.Amount;
            account.LastUpdated = DateTime.UtcNow;

            _context.Transactions.Add(tx);
            await _context.SaveChangesAsync();

            return Ok(new { balance = account.Balance });
        }

        private int GetUserId()
        {
          return HttpContext.Session.GetInt32("UserId") ?? 0;
        }
    }
}