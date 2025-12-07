namespace KioskAPI.Controllers
{
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.EntityFrameworkCore;
  using KioskAPI.Data;
  using System.Linq;
  using System.Threading.Tasks;
  using KioskAPI.Dtos;
  using Microsoft.AspNetCore.Identity;
  using KioskAPI.Models;

  [ApiController]
  [Route("api/[controller]")]
  [Authorize(Roles = "Admin")] // ALL routes protected
  public class AdminController : ControllerBase
  {
    private readonly UserManager<User> _usermanager;
    private readonly AppDbContext _context;

    public AdminController(UserManager<User> userManager, AppDbContext context)
    {
      this._usermanager = userManager;
      this._context = context;
    }

    // ===================== USERS =====================

    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] string? search = null,
    [FromQuery] string sortBy = "CreatedAt",
    [FromQuery] string sortOrder = "desc")
    {
      var query = this._usermanager.Users.AsQueryable();

      // SEARCH
      if (!string.IsNullOrEmpty(search))
      {
        query = query.Where(u =>
            u.UserName.Contains(search) ||
            u.Email.Contains(search));
      }

      // SORT
      query = sortBy.ToLower() switch
      {
        "name" => sortOrder == "asc" ? query.OrderBy(u => u.UserName) : query.OrderByDescending(u => u.UserName),
        "email" => sortOrder == "asc" ? query.OrderBy(u => u.Email) : query.OrderByDescending(u => u.Email),
        _ => sortOrder == "asc" ? query.OrderBy(u => u.CreatedAt) : query.OrderByDescending(u => u.CreatedAt),
      };

      var total = await query.CountAsync().ConfigureAwait(true);
      var users = await query.Skip((page - 1) * pageSize)
                             .Take(pageSize)
                             .ToListAsync()
                             .ConfigureAwait(true);

      // GET ROLES FOR EACH USER
      var userRolesList = new List<object>();
      foreach (var u in users)
      {
        var roles = await this._usermanager.GetRolesAsync(u); // List<string>
        userRolesList.Add(new
        {
          u.Id,
          Name = u.UserName,
          Email = u.Email,
          CreatedAt = u.CreatedAt,
          Roles = roles
        });
      }

      return this.Ok(new
      {
        total,
        page,
        pageSize,
        data = userRolesList
      });
    }

    // ===================== PRODUCTS =====================

    [HttpGet("products")]
    public async Task<IActionResult> GetAllProducts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string sortBy = "Price",
        [FromQuery] string sortOrder = "asc")
    {
      var query = this._context.Products.Include(p => p.Supplier).AsQueryable();

      if (!string.IsNullOrEmpty(search))
      {
        query = query.Where(p =>
            p.Name.Contains(search) ||
            p.Category.Contains(search) ||
            p.Supplier.Name.Contains(search));
      }

      query = sortBy.ToLower() switch
      {
        "name" => sortOrder == "asc" ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name),
        "price" => sortOrder == "asc" ? query.OrderBy(p => p.Price) : query.OrderByDescending(p => p.Price),
        "quantity" => sortOrder == "asc" ? query.OrderBy(p => p.Quantity) : query.OrderByDescending(p => p.Quantity),
        _ => query.OrderBy(p => p.ProductId)
      };

      var total = await query.CountAsync().ConfigureAwait(true);
      var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync().ConfigureAwait(true);

      return this.Ok(new
      {
        total,
        page,
        pageSize,
        data = items.Select(p => new
        {
          p.ProductId,
          p.Name,
          p.Description,
          p.Price,
          p.Category,
          p.Quantity,
          Supplier = p.Supplier.Name
        })
      });
    }

    // ===================== ORDERS =====================

    [HttpGet("orders")]
    public async Task<IActionResult> GetAllOrders(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null)
    {
      var query = this._context.Orders
          .Include(o => o.User)
          .Include(o => o.OrderItems)
          .AsQueryable();

      if (!string.IsNullOrEmpty(search))
      {
        query = query.Where(o => o.User.Name.Contains(search));
      }

      if (!string.IsNullOrEmpty(status))
      {
        query = query.Where(o => o.Status.Contains(status));
      }

      var total = await query.CountAsync().ConfigureAwait(true);
      var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync().ConfigureAwait(true);

      return this.Ok(new
      {
        total,
        page,
        pageSize,
        data = items.Select(o => new
        {
          o.OrderId,
          Customer = o.User.Name,
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
      });
    }

    // ===================== TRANSACTIONS =====================

    [HttpGet("transactions")]
    public async Task<IActionResult> GetAllTransactions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? type = null)
    {
      var query = this._context.Transactions
          .Include(t => t.Account)
          .ThenInclude(a => a.User)
          .AsQueryable();

      if (!string.IsNullOrEmpty(type))
      {
        query = query.Where(t => t.Type == type);
      }

      var total = await query.CountAsync().ConfigureAwait(true);
      var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync().ConfigureAwait(true);

      return this.Ok(new
      {
        total,
        page,
        pageSize,
        data = items.Select(t => new
        {
          t.TransactionId,
          AccountOwner = t.Account.User.Name,
          t.Type,
          t.TotalAmount,
          t.Description,
          t.CreatedAt
        })
      });
    }

    [HttpPost("topup")]
    public async Task<IActionResult> TopUpUser([FromBody] AdminTopUpDo dto)
    {
      if (!this.ModelState.IsValid)
      {
        return this.BadRequest(this.ModelState);
      }

      var user = await this._context.Users.FindAsync(dto.UserId).ConfigureAwait(true);
      if (user == null)
      {
        return this.NotFound(new { message = "User not found" });
      }

      // Find or create account
      var account = await this._context.Accounts
          .FirstOrDefaultAsync(a => a.UserId == dto.UserId).ConfigureAwait(true);
      if (account == null)
      {
        account = new Account
        {
          UserId = dto.UserId,
          Balance = 0
        };
        this._context.Accounts.Add(account);
      }

      if (dto.Amount > 1500)
      {
        return this.BadRequest(new { message = "Maximum amount to deposit is R1500" });
      }

      // Top up
      account.Balance += dto.Amount;

      // Log transaction
      var transaction = new Transaction
      {
        AccountId = account.AccountId,
        Type = "TopUp",
        TotalAmount = dto.Amount,
        Description = dto.Description ?? "Admin Top-Up",
        CreatedAt = DateTime.UtcNow
      };
      this._context.Transactions.Add(transaction);

      await this._context.SaveChangesAsync().ConfigureAwait(true);

      return this.Ok(new
      {
        message = $"Successfully topped up {dto.Amount:C} for {user.UserName}",
        newBalance = account.Balance
      });
    }
  }
}