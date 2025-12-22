namespace KioskAPI.Controllers
{
  using System;
  using System.Linq;
  using System.Security.Claims;
  using System.Threading.Tasks;
  using KioskAPI.Data;
  using KioskAPI.Dtos;
  using KioskAPI.Models;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.EntityFrameworkCore;

  /// <summary>
  /// Manages financial transactions for kiosk user accounts.
  /// Supports viewing transactions, creating credits/debits,
  /// and administrative transaction management.
  /// </summary>
  [ApiController]
  [Route("api/[controller]")]
  [Authorize]
  public class TransactionController : ControllerBase
  {
    private readonly AppDbContext _context;

    /// <summary>
    /// Initializes the TransactionController with database context.
    /// </summary>
    /// <param name="context">Application database context</param>
    public TransactionController(AppDbContext context)
    {
      this._context = context;
    }

    /// <summary>
    /// Retrieves all transactions in the system.
    /// Admin-only endpoint.
    /// </summary>
    /// <returns>List of all transactions with account and user details</returns>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllTransactions()
    {
      var transactions = await this._context.Transactions
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
          .ToListAsync().ConfigureAwait(true);

      return this.Ok(transactions);
    }

    /// <summary>
    /// Retrieves transactions belonging to the currently authenticated user.
    /// </summary>
    /// <returns>User’s transaction history</returns>
    [HttpGet("mytrasactions")]
    [Authorize]
    public async Task<IActionResult> GetMyTransactions()
    {
      var userIdClaim = this.User.FindFirst("id")?.Value ?? this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

      if (string.IsNullOrEmpty(userIdClaim))
      {
        return this.Unauthorized(new { message = "Invalid token: no user id found." });
      }

      int userId = int.Parse(userIdClaim);

      var user = await this._context.Users
          .Include(u => u.Account)
          .ThenInclude(a => a.Transactions)
          .FirstOrDefaultAsync(u => u.Id == userId)
          .ConfigureAwait(true);

      if (user == null)
      {
        return this.NotFound(new { message = "User not found." });
      }

      if (user.Account == null)
      {
        return this.NotFound(new { message = "User has no account." });
      }

      var transactions = user.Account.Transactions
          .OrderByDescending(t => t.CreatedAt)
          .Select(t => new
          {
            t.TransactionId,
            t.Type,
            t.TotalAmount,
            t.Description,
            t.CreatedAt
          })
          .ToList();

      return this.Ok(transactions);
    }

    /// <summary>
    /// Creates a new financial transaction (credit or debit).
    /// Automatically updates the account balance.
    /// </summary>
    /// <param name="dto">Transaction data transfer object</param>
    /// <returns>Transaction confirmation and updated balance</returns>
    [HttpPost]
    public async Task<IActionResult> CreateTransaction([FromBody] TransactionDto dto)
    {
      if (dto == null)
      {
        return this.BadRequest(new { message = "Transaction data is required." });
      }

      var account = await this._context.Accounts.FirstOrDefaultAsync(a => a.AccountId == dto.AccountId).ConfigureAwait(true);
      if (account == null)
      {
        return this.NotFound(new { message = "Account not found." });
      }

      var transaction = new Transaction
      {
        AccountId = dto.AccountId,
        Type = dto.Type,
        TotalAmount = dto.TotalAmount,
        Description = dto.Description ?? $"{dto.Type} of {dto.TotalAmount:C}",
        CreatedAt = DateTime.UtcNow
      };

      if (dto.Type.ToLower() == "credit")
      {
        account.Balance += dto.TotalAmount;
      }
      else if (dto.Type.ToLower() == "debit")
      {
        if (account.Balance < dto.TotalAmount)
        {
          return this.BadRequest(new { message = "Insufficient balance." });
        }

        account.Balance -= dto.TotalAmount;
      }
      else
      {
        return this.BadRequest(new { message = "Transaction type must be 'credit' or 'debit'." });
      }

      account.LastUpdated = DateTime.UtcNow;

      this._context.Transactions.Add(transaction);
      await this._context.SaveChangesAsync().ConfigureAwait(true);

      return this.Ok(new
      {
        message = "Transaction recorded successfully.",
        transaction.TransactionId,
        account.Balance
      });
    }

    /// <summary>
    /// Deletes a transaction and reverses its effect on the account balance.
    /// Admin-only endpoint.
    /// </summary>
    /// <param name="transactionId">Transaction identifier</param>
    /// <returns>Deletion confirmation</returns>
    [HttpDelete("{transactionId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteTransaction(int transactionId)
    {
      var transaction = await this._context.Transactions.FindAsync(transactionId).ConfigureAwait(true);
      if (transaction == null)
      {
        return this.NotFound(new { message = "Transaction not found." });
      }

      var account = await this._context.Accounts.FindAsync(transaction.AccountId).ConfigureAwait(true);
      if (account != null)
      {
        if (transaction.Type.ToLower() == "credit")
        {
          account.Balance -= transaction.TotalAmount;
        }
        else if (transaction.Type.ToLower() == "debit")
        {
          account.Balance += transaction.TotalAmount;
        }

        account.LastUpdated = DateTime.UtcNow;
      }

      this._context.Transactions.Remove(transaction);
      await this._context.SaveChangesAsync().ConfigureAwait(true);

      return this.Ok(new { message = "Transaction deleted and account balance adjusted." });
    }
  }
}
