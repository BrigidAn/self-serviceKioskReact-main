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

  [ApiController]
  [Route("api/[controller]")]
  [Authorize]
  public class CartController : ControllerBase
  {
    private readonly AppDbContext _context;
    private readonly ILogger<CartController> _logger;

    public CartController(AppDbContext context, ILogger<CartController> logger)
    {
      this._context = context;
      this._logger = logger;
    }

    // Helper: Get UserId from JWT
    private int GetIdentityUserId()
    {
      var claim = this.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
      return claim != null ? int.Parse(claim.Value) : 0;
    }

    // GET current cart
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
          .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsCheckedOut).ConfigureAwait(true);

      // Remove expired cart
      if (cart != null && cart.ExpiresAt < DateTime.UtcNow)
      {

        try
        {
          this._context.CartItems.RemoveRange(cart.CartItems);
          this._context.Carts.Remove(cart);
          await this._context.SaveChangesAsync().ConfigureAwait(true);
          await transaction.CommitAsync().ConfigureAwait(true);

          this._logger.LogInformation("Cart for user {UserId} expired and cleared", userId);
          return this.NotFound(new { message = "Cart expired and was cleared" });
        }
        catch (Exception ex)
        {
          await transaction.RollbackAsync().ConfigureAwait(true);
          this._logger.LogError(ex, "Error clearing expired cart for user {UserId}", userId);
          return this.StatusCode(500, new { message = "Error processing cart expiration" });
        }
      }

      if (cart == null)
      {
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
          ImageUrl = ci.Product?.ImageUrl ?? null,
          Quantity = ci.Quantity
        }).ToList(),
        TotalAmount = cart.CartItems.Sum(ci => ci.UnitPrice * ci.Quantity),
        ExpiresAt = cart.ExpiresAt
      };

      return this.Ok(cartDto);
    }

    // POST add to cart
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
          return this.BadRequest(new { message = "Product not found" });
        }

        if (dto.Quantity < 1)
        {
          return this.BadRequest(new { message = "Quantity must be at least 1" });
        }

        if (product.Quantity < dto.Quantity)
        {
          return this.BadRequest(new { message = "Not enough stock" });
        }

        var cart = await this._context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsCheckedOut).ConfigureAwait(true);

        if (cart != null && cart.ExpiresAt < DateTime.UtcNow)
        {
          this._context.CartItems.RemoveRange(cart.CartItems);
          this._context.Carts.Remove(cart);
          await this._context.SaveChangesAsync().ConfigureAwait(true);
          cart = null;
          this._logger.LogInformation("Expired cart cleared for user {UserId}", userId);
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
          cart.ExpiresAt = DateTime.UtcNow.AddHours(24); // extend expiry
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

        this._logger.LogInformation("Added product {ProductId} x {Quantity} to cart {CartId}", product.ProductId, dto.Quantity, cart.CartId);

        return this.Ok(new { message = "Item added to cart" });
      }
      catch (Exception ex)
      {
        await transaction.RollbackAsync().ConfigureAwait(true);
        this._logger.LogError(ex, "Error adding item to cart for user {UserId}", userId);
        return this.StatusCode(500, new { message = "Error adding item to cart" });
      }
    }

    // POST update quantity
    [HttpPost("item/{itemId}")]
    public async Task<IActionResult> UpdateQuantity(int itemId, [FromBody] UpdateQuantityDto dto)
    {
      if (dto.Quantity < 1)
      {
        return this.BadRequest(new { message = "Quantity must be at least 1." });
      }

      using var transaction = await this._context.Database.BeginTransactionAsync().ConfigureAwait(true);
      try
      {
        var item = await this._context.CartItems.Include(ci => ci.Product).FirstOrDefaultAsync(ci => ci.CartItemId == itemId).ConfigureAwait(true);
        if (item == null)
        {
          return this.NotFound(new { message = "Item not found." });
        }

        if (item.Product.Quantity + item.Quantity < dto.Quantity)
        {
          return this.BadRequest(new { message = "Not enough stock" });
        }

        item.Product.Quantity += item.Quantity - dto.Quantity;
        item.Quantity = dto.Quantity;

        await this._context.SaveChangesAsync().ConfigureAwait(true);
        await transaction.CommitAsync().ConfigureAwait(true);

        this._logger.LogInformation("Updated quantity for cart item {CartItemId} to {Quantity}", itemId, dto.Quantity);
        return this.Ok(new { message = "Quantity updated." });
      }
      catch (Exception ex)
      {
        await transaction.RollbackAsync().ConfigureAwait(true);
        this._logger.LogError(ex, "Error updating quantity for cart item {CartItemId}", itemId);
        return this.StatusCode(500, new { message = "Error updating quantity" });
      }
    }

    // DELETE cart item
    [HttpDelete("item/{cartItemId}")]
    public async Task<IActionResult> RemoveItem(int cartItemId)
    {
      using var transaction = await this._context.Database.BeginTransactionAsync().ConfigureAwait(true);
      try
      {
        var cartItem = await this._context.CartItems
            .Include(ci => ci.Product)
            .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId).ConfigureAwait(true);

        if (cartItem == null)
        {
          return this.NotFound(new { message = "Cart item not found." });
        }

        if (cartItem.Product != null)
        {
          cartItem.Product.Quantity += cartItem.Quantity;
        }

        this._context.CartItems.Remove(cartItem);
        await this._context.SaveChangesAsync().ConfigureAwait(true);
        await transaction.CommitAsync().ConfigureAwait(true);

        this._logger.LogInformation("Removed cart item {CartItemId} and restored stock", cartItemId);
        return this.Ok(new { message = "Item removed and stock restored." });
      }
      catch (Exception ex)
      {
        await transaction.RollbackAsync().ConfigureAwait(true);
        this._logger.LogError(ex, "Error removing cart item {CartItemId}", cartItemId);
        return this.StatusCode(500, new { message = "Error removing item" });
      }
    }

    // POST expire cart manually
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
            .FirstOrDefaultAsync(c => c.UserId == userId).ConfigureAwait(true);

        if (cart == null || !cart.CartItems.Any())
        {
          return this.Ok(new { message = "Cart already empty." });
        }

        foreach (var item in cart.CartItems)
        {
          item.Product.Quantity += item.Quantity;
        }

        cart.CartItems.Clear();
        await this._context.SaveChangesAsync().ConfigureAwait(true);
        await transaction.CommitAsync().ConfigureAwait(true);

        this._logger.LogInformation("Cart for user {UserId} expired manually", userId);
        return this.Ok(new { message = "Cart expired. Items returned to stock." });
      }
      catch (Exception ex)
      {
        await transaction.RollbackAsync().ConfigureAwait(true);
        this._logger.LogError(ex, "Error expiring cart for user {UserId}", userId);
        return this.StatusCode(500, new { message = "Error expiring cart" });
      }
    }
  }
}
