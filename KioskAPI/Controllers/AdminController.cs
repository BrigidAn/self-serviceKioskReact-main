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
    private readonly ILogger<AdminController> _logger;

    public AdminController(UserManager<User> userManager, AppDbContext context, ILogger<AdminController> logger)
    {
      this._usermanager = userManager;
      this._context = context;
      this._logger = logger;
    }

    // ===================== USERS =====================

    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] string? search = null)
    {
      this._logger.LogInformation("Admin requested all users. Page: {Page}, PageSize: {PageSize}, Search: {Search}", page, pageSize, search);
      var query = this._usermanager.Users.AsQueryable();

      if (!string.IsNullOrEmpty(search))
      {
        query = query.Where(u =>
            u.UserName.Contains(search) ||
            u.Email.Contains(search));
        this._logger.LogInformation("Filtering users by search term: {Search}", search);
      }

      var total = await query.CountAsync().ConfigureAwait(true);
      var users = await query.Skip((page - 1) * pageSize)
                             .Take(pageSize)
                             .ToListAsync()
                             .ConfigureAwait(true);

      this._logger.LogInformation("{Count} users retrieved from database", users.Count);

      var userRolesList = new List<object>();
      foreach (var u in users)
      {
        var roles = await this._usermanager.GetRolesAsync(u).ConfigureAwait(true); // List<string>

        // Get user's balance
        var account = await this._context.Accounts.FirstOrDefaultAsync(a => a.UserId == u.Id).ConfigureAwait(true);
        var balance = account?.Balance ?? 0;

        userRolesList.Add(new
        {
          u.Id,
          Name = u.UserName,
          Email = u.Email,
          CreatedAt = u.CreatedAt,
          Roles = roles,
          Balance = balance
        });
      }

      this._logger.LogInformation("Returning user data to client");
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
      this._logger.LogInformation("Admin requested all products. Page: {Page}, PageSize: {PageSize}, Search: {Search}", page, pageSize, search);
      var query = this._context.Products.Include(p => p.Supplier).AsQueryable();

      if (!string.IsNullOrEmpty(search))
      {
        query = query.Where(p =>
            p.Name.Contains(search) ||
            p.Category.Contains(search) ||
            p.Supplier.Name.Contains(search));
        this._logger.LogInformation("Filtering products by search term: {Search}", search);

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
      this._logger.LogInformation("{Count} products retrieved from database");

      this._logger.LogInformation("Returning product data to client");
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
      this._logger.LogInformation("Admin requested all orders. Page: {Page}, PageSize: {PageSize}, Search: {Search}", page, pageSize, search);

      var query = this._context.Orders
          .Include(o => o.User)
          .Include(o => o.OrderItems)
          .AsQueryable();

      if (!string.IsNullOrEmpty(search))
      {
        this._logger.LogInformation("Filtering orders by search term: {Search}", search);

        query = query.Where(o => o.User.Name.Contains(search));
      }

      if (!string.IsNullOrEmpty(status))
      {
        this._logger.LogInformation("Filtering orders by status term: {Search}", search);

        query = query.Where(o => o.Status.Contains(status));
      }

      var total = await query.CountAsync().ConfigureAwait(true);
      var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync().ConfigureAwait(true);
      this._logger.LogInformation("{Count} orders retrieved from database");

      this._logger.LogInformation("Returning user data to client");
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
      this._logger.LogInformation("Admin attempting to top up user {UserId} with amount {Amount}", dto.UserId, dto.Amount);
      if (!this.ModelState.IsValid)
      {
        this._logger.LogWarning("Invalid model state for top-up: {@ModelState}", ModelState);
        return this.BadRequest(this.ModelState);
      }

      // Check if user exists
      var user = await this._context.Users.FindAsync(dto.UserId).ConfigureAwait(true);
      if (user == null)
      {
        this._logger.LogWarning("User {UserId} not found for top-up", dto.UserId);
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
          Balance = 0,
          LastUpdated = DateTime.UtcNow
        };
        this._context.Accounts.Add(account);
        await this._context.SaveChangesAsync().ConfigureAwait(true); // Save to get AccountId
        this._logger.LogInformation("Created new account for user {UserId}", dto.UserId);
      }

      if (dto.Amount > 1500)
      {
        this._logger.LogWarning("Attempted top-up exceeds maximum: {Amount}", dto.Amount);
        return this.BadRequest(new { message = "Maximum amount to deposit is R1500" });
      }

      // Top up
      account.Balance += dto.Amount;
      account.LastUpdated = DateTime.UtcNow;

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
      this._logger.LogInformation("Successfully topped up {Amount} for user {UserId}", dto.Amount, dto.UserId);
      return this.Ok(new
      {
        message = $"Successfully topped up {dto.Amount:C} for {user.UserName}",
        newBalance = account.Balance
      });
    }

    [HttpPost("cart/add")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddToUserCart([FromBody] AdminAddToCartDto dto)
    {
      var user = await this._context.Users.FindAsync(dto.UserId).ConfigureAwait(true);
      if (user == null)
      {
        return this.NotFound(new { message = "User not found" });
      }

      var product = await this._context.Products.FindAsync(dto.ProductId).ConfigureAwait(true);
      if (product == null)
      {
        return this.BadRequest(new { message = "Product not found" });
      }

      if (dto.Quantity < 1 || product.Quantity < dto.Quantity)
      {
        return this.BadRequest(new { message = "Invalid quantity" });
      }

      var cart = await this._context.Carts
          .Include(c => c.CartItems)
          .FirstOrDefaultAsync(c => c.UserId == dto.UserId && !c.IsCheckedOut).ConfigureAwait(true);

      if (cart == null)
      {
        cart = new Cart { UserId = dto.UserId, ExpiresAt = DateTime.UtcNow.AddHours(24) };
        this._context.Carts.Add(cart);
        await this._context.SaveChangesAsync().ConfigureAwait(true);
      }

      var cartItem = new CartItem
      {
        CartId = cart.CartId,
        ProductId = product.ProductId,
        Quantity = dto.Quantity,
        UnitPrice = product.Price
      };

      this._context.CartItems.Add(cartItem);
      product.Quantity -= dto.Quantity;
      await this._context.SaveChangesAsync().ConfigureAwait(true);

      return this.Ok(new { message = "Item added to user's cart" });
    }

    [HttpGet("cart/summary/{userId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUserCartSummary(int userId)
    {
      var cart = await this._context.Carts
          .Include(c => c.CartItems)
          .ThenInclude(ci => ci.Product)
          .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsCheckedOut).ConfigureAwait(true);

      if (cart == null || !cart.CartItems.Any())
      {
        return this.NotFound(new { message = "Cart is empty" });
      }

      var itemsTotal = cart.CartItems.Sum(ci => ci.UnitPrice * ci.Quantity);

      return this.Ok(new
      {
        cartId = cart.CartId,
        items = cart.CartItems.Select(ci => new
        {
          ci.CartItemId,
          ci.ProductId,
          ProductName = ci.Product?.Name ?? "Unknown",
          ImageUrl = ci.Product?.ImageUrl ?? "",
          Quantity = ci.Quantity,
          UnitPrice = ci.UnitPrice
        }),
        itemsTotal
      });
    }
    [HttpPost("cart/checkout/{userId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CheckoutUserCart(int userId)
    {
      var cart = await _context.Carts
          .Include(c => c.CartItems)
          .ThenInclude(ci => ci.Product)
          .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsCheckedOut)
          .ConfigureAwait(true);

      if (cart == null || !cart.CartItems.Any())
        return NotFound(new { message = "Cart is empty" });

      // Create order
      var order = new Order
      {
        UserId = userId,
        OrderDate = DateTime.UtcNow,
        Status = "Pending",
        PaymentStatus = "Pending",
        TotalAmount = cart.CartItems.Sum(ci => ci.UnitPrice * ci.Quantity)
      };

      _context.Orders.Add(order);
      await _context.SaveChangesAsync().ConfigureAwait(true);

      // Create order items
      foreach (var ci in cart.CartItems)
      {
        _context.OrderItems.Add(new OrderItem
        {
          OrderId = order.OrderId,
          ProductId = ci.ProductId,
          Quantity = ci.Quantity,
          PriceAtPurchase = ci.UnitPrice
        });

        // Reduce product stock
        ci.Product.Quantity -= ci.Quantity;
      }

      // Mark cart as checked out
      cart.IsCheckedOut = true;
      await _context.SaveChangesAsync().ConfigureAwait(true);

      return Ok(new { message = "Checkout successful", orderId = order.OrderId });
    }

  }
}