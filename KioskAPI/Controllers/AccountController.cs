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

   
 // GET BALANCE (SESSION-BASED)
    [HttpGet("balance")]
    public async Task<IActionResult> GetBalance()
    {
        int? userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return Unauthorized(new { message = "Not logged in" });

        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.UserId == userId);

        return Ok(new { balance = account?.Balance ?? 0 });
    }

      
// TOP-UP onto ACCOUNT session-based
    [HttpPost("topup")]
    public async Task<IActionResult> TopUp([FromBody] decimal amount)
    {
        int? userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return Unauthorized();

        var account = await _context.Accounts.FirstOrDefaultAsync(a => a.UserId == userId);
        if (account == null) return NotFound();

        account.Balance += amount;

        // Create credit transaction
        _context.Transactions.Add(new Transaction
        {
            AccountId = account.AccountId,
            Type = "credit",
            TotalAmount = amount,
            Description = "Top-up",
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        return Ok(new { balance = account.Balance });
    }


         // GET USER TRANSACTIONS
    [HttpGet("transactions")]
    public async Task<IActionResult> GetMyTransactions()
    {
        int? userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return Unauthorized();

        var user = await _context.Users
            .Include(u => u.Account)
            .ThenInclude(a => a.Transactions)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        var tx = user.Account.Transactions.Select(t => new {
            t.TransactionId,
            t.Type,
            t.TotalAmount,
            t.Description,
            t.CreatedAt
        });

        return Ok(tx);
    }

        private int GetUserId()
        {
          return HttpContext.Session.GetInt32("UserId") ?? 0;
        }
    }
}