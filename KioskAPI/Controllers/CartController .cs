namespace KioskAPI.Controllers
{
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.EntityFrameworkCore;
  using KioskAPI.Data;
  using KioskAPI.Dtos;
  using KioskAPI.Models;
  using Microsoft.AspNetCore.Authorization;
  using System.Security.Claims;
  using Microsoft.Extensions.Logging;

  /// <summary>
  /// Handles shopping cart operations for authenticated users
  /// Includes cart retrieval, item management, stock handling, and cart expiration.
  /// </summary>
  [ApiController]
  [Route("api/[controller]")]
  [Authorize]
  public class CartController : ControllerBase
  {
    private readonly AppDbContext _context;
    private readonly ILogger<CartController> _logger;

    /// <summary>
    /// Intializes a new instance of the <see cref="CartController"/>
    /// </summary>
    /// <param name="context">Inject database context</param>
    /// <param name="logger">Inject logger instance</param>
    public CartController(AppDbContext context, ILogger<CartController> logger)
    {
      this._context = context;
      this._logger = logger;
    }

    /// <summary>
    /// Extracts the authenticated user's Id from JWT claim
    /// </summary>
    /// <returns>User Id if avaliable, otherwise 0.</returns>
    private int GetIdentityUserId()
    {
      var claim = this.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
      return claim != null ? int.Parse(claim.Value) : 0;
    }

    /// <summary>
    /// Retrieves the current active cart for the logged-in user.
    /// Automatically clears expired carts.
    /// </summary>
    /// <returns>
    /// 200 OK with cart details,
    /// 404 Not Found if cart does not exist or expired,
    /// 401 Unauthorized if user is not authenticated.
    /// </returns>
    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
      int userId = this.GetIdentityUserId();
      if (userId == 0)
      {
        return this.Unauthorized();
      }

      using var transaction = await this._context.Database.BeginTransactionAsync().ConfigureAwait(true);

      var cart = await this._context.Carts
          .Include(c => c.CartItems)
          .ThenInclude(ci => ci.Product)
          .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsCheckedOut)
          .ConfigureAwait(true);

      if (cart != null && cart.ExpiresAt < DateTime.UtcNow)
      {
        try
        {
          this._context.CartItems.RemoveRange(cart.CartItems);
          this._context.Carts.Remove(cart);
          await this._context.SaveChangesAsync().ConfigureAwait(true);
          await transaction.CommitAsync().ConfigureAwait(true);

          this._logger.LogInformation(
            "Cart expired and cleared for user {UserId}", userId);

          return this.NotFound(new { message = "Cart expired and was cleared" });
        }
        catch (Exception ex)
        {
          await transaction.RollbackAsync().ConfigureAwait(true);
          this._logger.LogError(
            ex, "Error clearing expired cart for user {UserId}", userId);

          return this.StatusCode(500, new { message = "Error processing cart expiration" });
        }
      }

      if (cart == null)
      {
        this._logger.LogWarning(
          "Cart not found for user {UserId}", userId);

        return this.NotFound(new { message = "Cart not found" });
      }

      var cartDto = new CartDto
      {
        CartId = cart.CartId,
        UserId = cart.UserId,
        Items = cart.CartItems.Select(ci => new CartItemDto
        {
          CartItemId = ci.CartItemId,
          ProductId = ci.ProductId,
          ProductName = ci.Product?.Name ?? "Unknown",
          UnitPrice = ci.UnitPrice,
          ImageUrl = ci.Product?.ImageUrl,
          Quantity = ci.Quantity
        }).ToList(),
        TotalAmount = cart.CartItems.Sum(ci => ci.UnitPrice * ci.Quantity),
        ExpiresAt = cart.ExpiresAt
      };

      return this.Ok(cartDto);
    }

    /// <summary>
    /// Adds a product to the authenticated user's cart.
    /// Automatically creates a new cart if one does not exist.
    /// </summary>
    /// <param name="dto">Product ID and quantity.</param>
    /// <returns>
    /// 200 OK if item added,
    /// 400 Bad Request if invalid input or insufficient stock,
    /// 401 Unauthorized if user not authenticated.
    /// </returns>
    [HttpPost("add")]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartDto dto)
    {
      int userId = this.GetIdentityUserId();
      if (userId == 0)
      {
        return this.Unauthorized();
      }

      using var transaction = await this._context.Database.BeginTransactionAsync().ConfigureAwait(true);
      try
      {
        var product = await this._context.Products.FindAsync(dto.ProductId).ConfigureAwait(true);
        if (product == null)
        {
          this._logger.LogWarning(
            "AddToCart failed: Product {ProductId} not found for user {UserId}",
            dto.ProductId, userId);

          return this.BadRequest(new { message = "Product not found" });
        }

        if (dto.Quantity < 1)
        {
          this._logger.LogWarning(
            "AddToCart failed: Invalid quantity {Quantity} for user {UserId}",
            dto.Quantity, userId);

          return this.BadRequest(new { message = "Quantity must be at least 1" });
        }

        if (product.Quantity < dto.Quantity)
        {
          this._logger.LogWarning(
            "AddToCart failed: Insufficient stock for product {ProductId} requested {Quantity} by user {UserId}",
            product.ProductId, dto.Quantity, userId);

          return this.BadRequest(new { message = "Not enough stock" });
        }

        var cart = await this._context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsCheckedOut)
            .ConfigureAwait(true);

        if (cart != null && cart.ExpiresAt < DateTime.UtcNow)
        {
          this._context.CartItems.RemoveRange(cart.CartItems);
          this._context.Carts.Remove(cart);
          await this._context.SaveChangesAsync().ConfigureAwait(true);
          cart = null;

          this._logger.LogInformation(
            "Expired cart cleared for user {UserId}", userId);
        }

        if (cart == null)
        {
          cart = new Cart
          {
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddHours(24)
          };
          this._context.Carts.Add(cart);
          await this._context.SaveChangesAsync().ConfigureAwait(true);
        }
        else
        {
          cart.ExpiresAt = DateTime.UtcNow.AddHours(24);
        }

        var cartItem = new CartItem
        {
          CartId = cart.CartId,
          ProductId = product.ProductId,
          Quantity = dto.Quantity,
          UnitPrice = product.Price,
        };

        this._context.CartItems.Add(cartItem);
        product.Quantity -= dto.Quantity;

        await this._context.SaveChangesAsync().ConfigureAwait(true);
        await transaction.CommitAsync().ConfigureAwait(true);

        this._logger.LogInformation(
          "User {UserId} added product {ProductId} x {Quantity} to cart {CartId}",
          userId, product.ProductId, dto.Quantity, cart.CartId);

        return this.Ok(new { message = "Item added to cart" });
      }
      catch (Exception ex)
      {
        await transaction.RollbackAsync().ConfigureAwait(true);
        this._logger.LogError(
          ex, "Error adding item to cart for user {UserId}", userId);

        return this.StatusCode(500, new { message = "Error adding item to cart" });
      }
    }

    /// <summary>
    /// Updates the quantity of an existing cart item.
    /// </summary>
    /// <param name="itemId">Cart item ID.</param>
    /// <param name="dto">New quantity.</param>
    /// <returns>
    /// 200 OK if updated,
    /// 400 Bad Request if quantity invalid or insufficient stock,
    /// 404 Not Found if cart item does not exist.
    /// </returns>
    [HttpPost("item/{itemId}")]
    public async Task<IActionResult> UpdateQuantity(int itemId, [FromBody] UpdateQuantityDto dto)
    {
      if (dto.Quantity < 1)
      {
        this._logger.LogWarning(
          "UpdateQuantity failed: Invalid quantity {Quantity} for cart item {CartItemId}",
          dto.Quantity, itemId);

        return this.BadRequest(new { message = "Quantity must be at least 1." });
      }

      using var transaction = await this._context.Database.BeginTransactionAsync().ConfigureAwait(true);
      try
      {
        var item = await this._context.CartItems
          .Include(ci => ci.Product)
          .Include(ci => ci.Cart)
          .FirstOrDefaultAsync(ci => ci.CartItemId == itemId)
          .ConfigureAwait(true);

        if (item == null)
        {
          this._logger.LogWarning(
            "UpdateQuantity failed: Cart item {CartItemId} not found",
            itemId);

          return this.NotFound(new { message = "Item not found." });
        }

        if (item.Product.Quantity + item.Quantity < dto.Quantity)
        {
          this._logger.LogWarning(
            "UpdateQuantity failed: Insufficient stock for product {ProductId} requested {Quantity} by user {UserId}",
            item.ProductId, dto.Quantity, item.Cart.UserId);

          return this.BadRequest(new { message = "Not enough stock" });
        }

        item.Product.Quantity += item.Quantity - dto.Quantity;
        item.Quantity = dto.Quantity;

        await this._context.SaveChangesAsync().ConfigureAwait(true);
        await transaction.CommitAsync().ConfigureAwait(true);

        this._logger.LogInformation(
          "User {UserId} updated cart item {CartItemId} to quantity {Quantity}",
          item.Cart.UserId, itemId, dto.Quantity);

        return this.Ok(new { message = "Quantity updated." });
      }
      catch (Exception ex)
      {
        await transaction.RollbackAsync().ConfigureAwait(true);
        this._logger.LogError(
          ex, "Error updating quantity for cart item {CartItemId}",
          itemId);

        return this.StatusCode(500, new { message = "Error updating quantity" });
      }
    }

    /// <summary>
    /// Removes an item from the cart and restores product stock.
    /// </summary>
    /// <param name="cartItemId">Cart item ID.</param>
    /// <returns>
    /// 200 OK if removed,
    /// 404 Not Found if item does not exist.
    /// </returns>
    [HttpDelete("item/{cartItemId}")]
    public async Task<IActionResult> RemoveItem(int cartItemId)
    {
      using var transaction = await this._context.Database.BeginTransactionAsync().ConfigureAwait(true);
      try
      {
        var cartItem = await this._context.CartItems
            .Include(ci => ci.Product)
            .Include(ci => ci.Cart)
            .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId)
            .ConfigureAwait(true);

        if (cartItem == null)
        {
          this._logger.LogWarning(
            "RemoveItem failed: Cart item {CartItemId} not found",
            cartItemId);

          return this.NotFound(new { message = "Cart item not found." });
        }

        if (cartItem.Product != null)
        {
          cartItem.Product.Quantity += cartItem.Quantity;
        }

        this._context.CartItems.Remove(cartItem);
        await this._context.SaveChangesAsync().ConfigureAwait(true);
        await transaction.CommitAsync().ConfigureAwait(true);

        this._logger.LogInformation(
          "User {UserId} removed cart item {CartItemId} and restored stock",
          cartItem.Cart.UserId, cartItemId);

        return this.Ok(new { message = "Item removed and stock restored." });
      }
      catch (Exception ex)
      {
        await transaction.RollbackAsync().ConfigureAwait(true);
        this._logger.LogError(
          ex, "Error removing cart item {CartItemId}",
          cartItemId);

        return this.StatusCode(500, new { message = "Error removing item" });
      }
    }

    /// <summary>
    /// Manually expires the user's cart and returns all items to stock.
    /// </summary>
    /// <returns>
    /// 200 OK when cart is cleared,
    /// 500 Internal Server Error if operation fails.
    /// </returns>
    [HttpPost("expire")]
    public async Task<IActionResult> ExpireCart()
    {
      int userId = this.GetIdentityUserId();
      using var transaction = await this._context.Database.BeginTransactionAsync().ConfigureAwait(true);
      try
      {
        var cart = await this._context.Carts
            .Include(c => c.CartItems)
            .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId)
            .ConfigureAwait(true);

        if (cart == null || !cart.CartItems.Any())
        {
          this._logger.LogInformation(
            "ExpireCart skipped: Cart already empty for user {UserId}",
            userId);

          return this.Ok(new { message = "Cart already empty." });
        }

        foreach (var item in cart.CartItems)
        {
          item.Product.Quantity += item.Quantity;
        }

        cart.CartItems.Clear();
        await this._context.SaveChangesAsync().ConfigureAwait(true);
        await transaction.CommitAsync().ConfigureAwait(true);

        this._logger.LogInformation(
          "Cart expired manually for user {UserId}",
          userId);

        return this.Ok(new { message = "Cart expired. Items returned to stock." });
      }
      catch (Exception ex)
      {
        await transaction.RollbackAsync().ConfigureAwait(true);
        this._logger.LogError(
          ex, "Error expiring cart for user {UserId}",
          userId);

        return this.StatusCode(500, new { message = "Error expiring cart" });
      }
    }
  }
}
