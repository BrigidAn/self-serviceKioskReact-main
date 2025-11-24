namespace KioskAPI.Repository
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using KioskAPI.Data;
  using KioskAPI.interfaces;
  using KioskAPI.Models;
  using Microsoft.EntityFrameworkCore;

  public class AccountRepository : IAccountRepository
  {
    private readonly AppDbContext _context;

    public AccountRepository(AppDbContext context)
    {
      this._context = context;
    }
    public async Task<Account?> GetAccountByUserIdAsync(int userId)
    {
      return await this._context.Accounts.FirstOrDefaultAsync(a => a.UserId == userId).ConfigureAwait(true);
    }

    public async Task<IEnumerable<Transaction>> GetTransactionsAsync(int userId)
    {
      return await this._context.Transactions
      .Where(t => t.Account.UserId == userId)
      .OrderByDescending(t => t.CreatedAt)
      .ToListAsync().ConfigureAwait(true);
    }

    public async Task UpdateBalanceAsync(int userId, decimal amount)
    {
      var account = await this.GetAccountByUserIdAsync(userId).ConfigureAwait(true);

      account.Balance += amount;
      account.LastUpdated = DateTime.UtcNow;

      this._context.Transactions.Add(new Transaction
      {
        AccountId = account.AccountId,
        Type = "credit",
        TotalAmount = amount,
        Description = "Top-up",
        CreatedAt = DateTime.UtcNow
      });

      await this._context.SaveChangesAsync().ConfigureAwait(true);
    }

    public async Task<Account> CreateAccountForUserAsync(int userId)
    {
      var account = new Account
      {
        UserId = userId,
        Balance = 0,
        LastUpdated = DateTime.UtcNow
      };

      this._context.Accounts.Add(account);
      await this._context.SaveChangesAsync().ConfigureAwait(true);
      return account;
    }

  }
}