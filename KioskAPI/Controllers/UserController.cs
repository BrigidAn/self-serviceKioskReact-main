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

  [ApiController]
  [Route("api/[controller]")]
  [Authorize] // All endpoints require authentication
  public class UserController : ControllerBase
  {
    private readonly AppDbContext _context;

    public UserController(AppDbContext context)
    {
      this._context = context;
    }

    // Helper: get current user's ID from JWT
    private int GetJwtUserId()
    {
      return int.Parse(this.User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
    }

    // GET: /api/user/profile
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

    // GET: /api/user/account
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

    // POST: /api/user/account/topup
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

    // ADMIN: Get any user's profile by userId
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

    // ADMIN: Get any user's account by userId
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
