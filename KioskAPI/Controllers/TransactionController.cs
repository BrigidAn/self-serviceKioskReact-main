using System;
using System.Collections.Generic;
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
    public class TransactionController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TransactionController(AppDbContext context)
        {
            _context = context;
        }

       // Get all transactions (admin view)
        [HttpGet]
        public async Task<IActionResult> GetAllTransactions()
        {
            var transactions = await _context.Transactions
                .Include(t => t.Account)
                .ThenInclude(a => a.User)
                .Select(t => new
                {
                    t.TransactionId,
                    AccountOwner = t.Account.User.Name,
                    t.AccountId,
                    t.Type,
                    t.TotalAmount,
                    t.Description,
                    t.CreatedAt
                })
                .ToListAsync();

            return Ok(transactions);
        }

        // Get transactions belonging to a specific user
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserTransactions(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Account)
                .ThenInclude(a => a.Transactions)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return NotFound(new { message = "User not found." });

            var transactions = user.Account?.Transactions
                .Select(t => new
                {
                    t.TransactionId,
                    t.Type,
                    t.TotalAmount,
                    t.Description,
                    t.CreatedAt
                })
                .ToList();

            return Ok(transactions);
        }

        

        // Create a new transaction (credit or debit)
      [HttpPost]
        public async Task<IActionResult> CreateTransaction([FromBody] TransactionDto dto)
        {
            if (dto == null)
                return BadRequest(new { message = "Transaction data is required." });

            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.AccountId == dto.AccountId);

            if (account == null)
                return NotFound(new { message = "Account not found." });

            // Create real transaction
            var transaction = new Transaction
            {
                AccountId = dto.AccountId,
                Type = dto.Type,
                TotalAmount = dto.TotalAmount,
                Description = dto.Description ?? $"{dto.Type} of {dto.TotalAmount:C}",
                CreatedAt = DateTime.UtcNow
            };

            // Apply credit/debit
            if (dto.Type.ToLower() == "credit")
            {
                account.Balance += dto.TotalAmount;
            }
            else if (dto.Type.ToLower() == "debit")
            {
                if (account.Balance < dto.TotalAmount)
                    return BadRequest(new { message = "Insufficient balance." });

                account.Balance -= dto.TotalAmount;
            }
            else
            {
                return BadRequest(new { message = "Transaction type must be 'credit' or 'debit'." });
            }

            account.LastUpdated = DateTime.UtcNow;

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Transaction recorded successfully.",
                transaction.TransactionId,
                account.Balance
            });
        }


        // Delete a transaction (admin use)
        [HttpDelete("{transactionId}")]
        public async Task<IActionResult> DeleteTransaction(int transactionId)
        {
            var transaction = await _context.Transactions.FindAsync(transactionId);
            if (transaction == null)
                return NotFound(new { message = "Transaction not found." });

            // Optionally reverse balance changes
            var account = await _context.Accounts.FindAsync(transaction.AccountId);
            if (account != null)
            {
                if (transaction.Type.ToLower() == "credit")
                    account.Balance -= transaction.TotalAmount;
                else if (transaction.Type.ToLower() == "debit")
                    account.Balance += transaction.TotalAmount;

                account.LastUpdated = DateTime.UtcNow;
            }

            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Transaction deleted and account balance adjusted." });
        }
    }
}