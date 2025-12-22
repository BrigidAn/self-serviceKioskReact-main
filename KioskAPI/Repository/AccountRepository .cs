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

  /// <summary>
  /// Implements <see cref="IAccountRepository"/> for managing user accounts and related transactions.
  /// </summary>
  public class AccountRepository : IAccountRepository
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="AccountRepository"/> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    private readonly AppDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="AccountRepository"/> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public AccountRepository(AppDbContext context)
    {
      this._context = context;
    }

    /// <summary>
    /// Retrieves all transactions associated with a user's account.
    /// </summary>
    /// <param name="userId">The ID of the user whose transactions are retrieved.</param>
    /// <returns>An enumerable of <see cref="Transaction"/> ordered by creation date descending.</returns>
    public async Task<Account?> GetAccountByUserIdAsync(int userId)
    {
      return await this._context.Accounts.FirstOrDefaultAsync(a => a.UserId == userId).ConfigureAwait(true);
    }

    /// <summary>
    /// Retrieves all transactions associated with a user's account.
    /// </summary>
    /// <param name="userId">The ID of the user whose transactions are retrieved.</param>
    /// <returns>An enumerable of <see cref="Transaction"/> ordered by creation date descending.</returns>
    public async Task<IEnumerable<Transaction>> GetTransactionsAsync(int userId)
    {
      return await this._context.Transactions
      .Where(t => t.Account.UserId == userId)
      .OrderByDescending(t => t.CreatedAt)
      .ToListAsync().ConfigureAwait(true);
    }

    /// <summary>
    /// Updates the account balance for a user and records the transaction as a top-up.
    /// </summary>
    /// <param name="userId">The ID of the user whose balance is updated.</param>
    /// <param name="amount">The amount to add to the account balance.</param>
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

    /// <summary>
    /// Creates a new account for a user with an initial balance of zero.
    /// </summary>
    /// <param name="userId">The ID of the user for whom the account is created.</param>
    /// <returns>The newly created <see cref="Account"/>.</returns>
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