namespace KioskAPI.Controllers
{
  using System;
  using System.Linq;
  using System.Threading.Tasks;
  using KioskAPI.Data;
  using KioskAPI.Dtos;
  using KioskAPI.Models;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.EntityFrameworkCore;
  using System.Security.Claims;

  /// <summary>
  /// Handles user profile and account operations.
  /// Supports viewing profile, account details, top-ups,
  /// and admin-level user/account management.
  /// </summary>
  [ApiController]
  [Route("api/[controller]")]
  [Authorize]
  public class UserController : ControllerBase
  {
    private readonly AppDbContext _context;

    /// <summary>
    /// Initializes the UserController with the database
    /// </summary>
    /// <param name="context">Application database context</param>
    public UserController(AppDbContext context)
    {
      this._context = context;
    }

    /// <summary>
    /// Extracts the authenticated user's ID from JWT claims.
    /// </summary>
    /// <returns>User Id as integer</returns>
    private int GetJwtUserId()
    {
      return int.Parse(this.User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
    }

    /// <summary>
    /// Retrieves the authenticated user's profile.
    /// </summary>
    /// <returns>User profile information</returns>
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
      int userId = this.GetJwtUserId();
      var user = await this._context.Users.FindAsync(userId).ConfigureAwait(true);
      if (user == null)
      {
        return this.NotFound(new { message = "User not found." });
      }

      return this.Ok(new
      {
        user.Id,
        user.Name,
        user.Email,
        user.CreatedAt
      });
    }

    /// <summary>
    ///  Retrieves the authenticated user's account and transaction history.
    /// </summary>
    /// <returns>Account details including transactions</returns>
    [HttpGet("account")]
    public async Task<IActionResult> GetAccount()
    {
      int userId = this.GetJwtUserId();
      var account = await this._context.Accounts
          .Include(a => a.Transactions)
          .Include(a => a.User)
          .FirstOrDefaultAsync(a => a.User.Id == userId).ConfigureAwait(true);

      if (account == null)
      {
        return this.NotFound(new { message = "Account not found." });
      }

      return this.Ok(new
      {
        account.AccountId,
        account.Balance,
        account.LastUpdated,
        Transactions = account.Transactions.Select(t => new
        {
          t.TransactionId,
          t.Type,
          t.TotalAmount,
          t.Description,
          t.CreatedAt
        })
      });
    }

    /// <summary>
    /// Tops up the authenticated user's account balance.
    /// </summary>
    /// <param name="data">Top-up amount</param>
    /// <returns>Confirmation and updated balance</returns>
    [HttpPost("account/topup")]
    public async Task<IActionResult> TopUp([FromBody] TopUpDto data)
    {
      if (data.Amount <= 0)
      {
        return this.BadRequest(new { message = "Amount must be greater than zero." });
      }

      int userId = this.GetJwtUserId();
      var account = await this._context.Accounts
          .Include(a => a.User)
          .FirstOrDefaultAsync(a => a.User.Id == userId).ConfigureAwait(true);

      if (account == null)
      {
        return this.NotFound(new { message = "Account not found." });
      }

      if (account.Balance + data.Amount > 100000)
      {
        return this.BadRequest(new { message = "Balance cannot exceed R100,000." });
      }

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

      this._context.Transactions.Add(transaction);
      await this._context.SaveChangesAsync().ConfigureAwait(true);

      return this.Ok(new { message = "Balance topped up successfully", balance = account.Balance });
    }

    /// <summary>
    /// Retrieves a specific user's profile (Admin-only).
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>User profile information</returns>
    [HttpGet("profile/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUserProfileAdmin(int id)
    {
      var user = await this._context.Users.FindAsync(id).ConfigureAwait(true);
      if (user == null)
      {
        return this.NotFound(new { message = "User not found." });
      }

      return this.Ok(new
      {
        user.Id,
        user.Name,
        user.Email,
        user.CreatedAt
      });
    }

    /// <summary>
    /// Retrieves a specific user's account and transactions (Admin-only).
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>Account details and transaction history</returns>
    [HttpGet("account/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAccountAdmin(int id)
    {
      var account = await this._context.Accounts
          .Include(a => a.Transactions)
          .Include(a => a.User)
          .FirstOrDefaultAsync(a => a.User.Id == id).ConfigureAwait(true);

      if (account == null)
      {
        return this.NotFound(new { message = "Account not found." });
      }

      return this.Ok(new
      {
        account.AccountId,
        account.Balance,
        account.LastUpdated,
        Transactions = account.Transactions.Select(t => new
        {
          t.TransactionId,
          t.Type,
          t.TotalAmount,
          t.Description,
          t.CreatedAt
        })
      });
    }
  }
}
