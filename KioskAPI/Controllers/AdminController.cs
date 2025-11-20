namespace KioskAPI.Controllers
{
  using System;
  using System.Linq;
  using System.Threading.Tasks;
  using KioskAPI.Data;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.EntityFrameworkCore;

  [ApiController]
  [Route("api/[controller]")]
  public class AdminController : ControllerBase
  {
    private readonly AppDbContext _context;

    public AdminController(AppDbContext context)
    {
      this._context = context;
    }

    private async Task<bool> IsAdminAsync(int userId)
    {
      var user = await this._context.Users
          .Include(u => u.Role)
          .FirstOrDefaultAsync(u => u.UserId == userId).ConfigureAwait(true);

      return user?.Role?.RoleName == "Admin";
    }

    // ✅ PAGINATED + SEARCHABLE USERS
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers(
        [FromQuery] int adminId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = "CreatedAt",
        [FromQuery] string? sortOrder = "desc")
    {
      if (!await this.IsAdminAsync(adminId).ConfigureAwait(true))
      {
        return this.Unauthorized(new { message = "Access denied. Admins only." });
      }

      var query = this._context.Users.Include(u => u.Role).AsQueryable();

      // Search by name or email
      if (!string.IsNullOrEmpty(search))
      {
        query = query.Where(u => u.Name.Contains(search) || u.Email.Contains(search));
      }

      // Sorting dynamically
      query = sortBy?.ToLower() switch
      {
        "name" => sortOrder == "asc" ? query.OrderBy(u => u.Name) : query.OrderByDescending(u => u.Name),
        "email" => sortOrder == "asc" ? query.OrderBy(u => u.Email) : query.OrderByDescending(u => u.Email),
        _ => sortOrder == "asc" ? query.OrderBy(u => u.CreatedAt) : query.OrderByDescending(u => u.CreatedAt)
      };

      var totalItems = await query.CountAsync().ConfigureAwait(true);
      var users = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync().ConfigureAwait(true);

      return this.Ok(new
      {
        totalItems,
        totalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
        currentPage = page,
        users = users.Select(u => new
        {
          u.UserId,
          u.Name,
          u.Email,
          Role = u.Role.RoleName,
          u.CreatedAt
        })
      });
    }

    // ✅ PAGINATED + SEARCHABLE PRODUCTS
    [HttpGet("products")]
    public async Task<IActionResult> GetAllProducts(
        [FromQuery] int adminId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = "Price",
        [FromQuery] string? sortOrder = "asc")
    {
      if (!await this.IsAdminAsync(adminId).ConfigureAwait(true))
      {
        return this.Unauthorized(new { message = "Access denied. Admins only." });
      }

      var query = this._context.Products.Include(p => p.Supplier).AsQueryable();

      // 🔍 Search by name, category, or supplier
      if (!string.IsNullOrEmpty(search))
      {
        query = query.Where(p => p.Name.Contains(search) || p.Category.Contains(search) || p.Supplier.Name.Contains(search));
      }

      // ↕️ Sorting dynamically
      query = sortBy?.ToLower() switch
      {
        "name" => sortOrder == "asc" ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name),
        "price" => sortOrder == "asc" ? query.OrderBy(p => p.Price) : query.OrderByDescending(p => p.Price),
        "quantity" => sortOrder == "asc" ? query.OrderBy(p => p.Quantity) : query.OrderByDescending(p => p.Quantity),
        _ => query.OrderBy(p => p.ProductId)
      };

      var totalItems = await query.CountAsync().ConfigureAwait(true);
      var products = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync().ConfigureAwait(true);

      return this.Ok(new
      {
        totalItems,
        totalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
        currentPage = page,
        products = products.Select(p => new
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

    // ✅ PAGINATED + FILTERABLE ORDERS
    [HttpGet("orders")]
    public async Task<IActionResult> GetAllOrders(
        [FromQuery] int adminId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null)
    {
      if (!await this.IsAdminAsync(adminId).ConfigureAwait(true))
      {
        return this.Unauthorized(new { message = "Access denied. Admins only." });
      }

      var query = this._context.Orders
          .Include(o => o.User)
          .Include(o => o.OrderItems)
          .AsQueryable();

      // 🔍 Filter by user name or order status
      if (!string.IsNullOrEmpty(search))
      {
        query = query.Where(o => o.User.Name.Contains(search));
      }

      if (!string.IsNullOrEmpty(status))
      {
        query = query.Where(o => o.Status.Contains(status));
      }

      var totalItems = await query.CountAsync().ConfigureAwait(true);
      var orders = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync().ConfigureAwait(true);

      return this.Ok(new
      {
        totalItems,
        totalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
        currentPage = page,
        orders = orders.Select(o => new
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

    // ✅ PAGINATED TRANSACTIONS
    [HttpGet("transactions")]
    public async Task<IActionResult> GetAllTransactions(
        [FromQuery] int adminId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? type = null)
    {
      if (!await this.IsAdminAsync(adminId).ConfigureAwait(true))
      {
        return this.Unauthorized(new { message = "Access denied. Admins only." });
      }

      var query = this._context.Transactions
          .Include(t => t.Account)
          .ThenInclude(a => a.User)
          .AsQueryable();

      // 🔍 Filter by type (credit/debit)
      if (!string.IsNullOrEmpty(type))
      {
        query = query.Where(t => t.Type == type);
      }

      var totalItems = await query.CountAsync().ConfigureAwait(true);
      var transactions = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync().ConfigureAwait(true);

      return this.Ok(new
      {
        totalItems,
        totalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
        currentPage = page,
        transactions = transactions.Select(t => new
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
  }
}
