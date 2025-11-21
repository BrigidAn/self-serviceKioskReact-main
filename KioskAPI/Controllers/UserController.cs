namespace KioskAPI.Controllers
{
  using System;
  using System.Linq;
  using System.Threading.Tasks;
  using KioskAPI.Data;
  using KioskAPI.Dtos;
  using KioskAPI.Models;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.EntityFrameworkCore;

  [ApiController]
  [Route("api/[controller]")]
  public class UserController : ControllerBase
  {
    private readonly AppDbContext _context;

    public UserController(AppDbContext context)
    {
      this._context = context;
    }

    // Get user profile by Identity Id
    [HttpGet("profile/{id}")]
    public async Task<IActionResult> GetUserProfile(int id)
    {
      var user = await this._context.Users
          .FirstOrDefaultAsync(u => u.Id == id)
          .ConfigureAwait(true);

      if (user == null)
      {
        return this.NotFound(new { message = "User not found" });
      }

      return this.Ok(new
      {
        user.Id,
        user.Name,
        user.Email,
        user.CreatedAt
      });
    }

    // Get account details by Identity Id
    [HttpGet("account/{id}")]
    public async Task<IActionResult> GetAccountDetails(int id)
    {
      var account = await this._context.Accounts
          .Include(a => a.Transactions)
          .Include(a => a.User)
          .FirstOrDefaultAsync(a => a.User.Id == id)
          .ConfigureAwait(true);

      if (account == null)
      {
        return this.NotFound(new { message = "Account not found" });
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

    // Top-up balance using Identity Id
    [HttpPost("account/topup")]
    public async Task<IActionResult> TopUpBalance([FromBody] TopUpDto data)
    {
      if (data.Amount <= 0)
      {
        return this.BadRequest(new { message = "Amount must be greater than zero." });
      }

      var account = await this._context.Accounts
          .Include(a => a.User)
          .FirstOrDefaultAsync(a => a.User.Id == data.Id)
          .ConfigureAwait(true);

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

      return this.Ok(new
      {
        message = "Balance topped up successfully",
        balance = account.Balance
      });
    }

    // Get orders by Identity Id
    [HttpGet("orders/{id}")]
    public async Task<IActionResult> GetUserOrders(int id)
    {
      var orders = await this._context.Orders
          .Include(o => o.OrderItems)
          .Include(o => o.User)
          .Where(o => o.User.Id == id)
          .Select(o => new
          {
            o.OrderId,
            o.OrderDate,
            o.TotalAmount,
            o.Status,
            o.PaymentStatus,
            Items = o.OrderItems.Select(i => new
            {
              i.ProductId,
              i.Quantity,
              i.PriceAtPurchase
            })
          })
          .ToListAsync()
          .ConfigureAwait(true);

      if (!orders.Any())
      {
        return this.NotFound(new { message = "No orders found for this user" });
      }

      return this.Ok(orders);
    }

    // Get transaction history by Identity Id
    [HttpGet("account/{id}/transactions")]
    public async Task<IActionResult> GetTransactionHistory(int id)
    {
      var account = await this._context.Accounts
          .Include(a => a.User)
          .FirstOrDefaultAsync(a => a.User.Id == id)
          .ConfigureAwait(true);

      if (account == null)
      {
        return this.NotFound(new { message = "Account not found." });
      }

      var transactions = await this._context.Transactions
          .Where(t => t.AccountId == account.AccountId)
          .OrderByDescending(t => t.CreatedAt)
          .ToListAsync()
          .ConfigureAwait(true);

      return this.Ok(transactions);
    }
  }
}
