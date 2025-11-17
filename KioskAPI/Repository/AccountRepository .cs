using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KioskAPI.Data;
using KioskAPI.interfaces;
using KioskAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace KioskAPI.Repository
{
    public class AccountRepository : IAccountRepository
    {
        private readonly AppDbContext _context;

        public AccountRepository (AppDbContext context)
        {
            _context = context;
        }
        public async Task<Account?> GetAccountByUserIdAsync(int userId)
        {
             return await _context.Accounts.FirstOrDefaultAsync(a => a.UserId == userId);
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsAsync(int userId)
        {
            return await _context.Transactions
            .Where(t => t.Account.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
        }

        public async Task UpdateBalanceAsync(int userId, decimal amount)
        {
             var account = await GetAccountByUserIdAsync(userId);

                account.Balance += amount;
                account.LastUpdated = DateTime.UtcNow;

                _context.Transactions.Add(new Transaction
                {
                    AccountId = account.AccountId,
                    Type = "credit",
                    TotalAmount = amount,
                    Description = "Top-up",
                    CreatedAt = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
        }
    }
}