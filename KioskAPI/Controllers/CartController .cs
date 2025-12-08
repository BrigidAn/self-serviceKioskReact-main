namespace KioskAPI.Controllers
{
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.EntityFrameworkCore;
  using KioskAPI.Data;
  using KioskAPI.Dtos;
  using KioskAPI.Models;
  using Microsoft.AspNetCore.Authorization;
  using System.Security.Claims;

  [ApiController]
  [Route("api/[controller]")]
  [Authorize]
  public class CartController : ControllerBase
  {
    private readonly AppDbContext _context;

    public CartController(AppDbContext context)
    {
      this._context = context;
    }

    //Get userId from the Jwt token
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

      var cart = await this._context.Carts
          .Include(c => c.CartItems)
          .ThenInclude(ci => ci.Product)
          .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsCheckedOut).ConfigureAwait(true);

      // ========== CART EXPIRED? ==========
      if (cart != null && cart.ExpiresAt < DateTime.UtcNow)
      {
        this._context.CartItems.RemoveRange(cart.CartItems);
        this._context.Carts.Remove(cart);
        await this._context.SaveChangesAsync().ConfigureAwait(true);

        return this.NotFound(new { message = "Cart expired and was cleared" });
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
      // Get the current user from JWT
      int userId = this.GetIdentityUserId();
      if (userId == 0)
      {
        return this.Unauthorized(new { message = "User not authenticated" });
      }

      // Find the product
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

      // Get or create cart for the user
      var cart = await this._context.Carts
          .Include(c => c.CartItems)
          .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsCheckedOut).ConfigureAwait(true);

      if (cart != null && cart.ExpiresAt < DateTime.UtcNow)
      {
        // Remove expired cart
        this._context.CartItems.RemoveRange(cart.CartItems);
        this._context.Carts.Remove(cart);
        await this._context.SaveChangesAsync().ConfigureAwait(true);
        cart = null;
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
        // Extend expiry on each add
        cart.ExpiresAt = DateTime.UtcNow.AddHours(24);
      }

      // Add item
      var cartItem = new CartItem
      {
        CartId = cart.CartId,
        ProductId = product.ProductId,
        Quantity = dto.Quantity,
        UnitPrice = product.Price
      };

      this._context.CartItems.Add(cartItem);

      // Reduce stock
      product.Quantity -= dto.Quantity;

      await this._context.SaveChangesAsync().ConfigureAwait(true);

      return this.Ok(new { message = "Item added to cart" });
    }

    //api/cart/item/{itemId}
    [HttpPost("item/{itemId}")]
    public async Task<IActionResult> UpdateQuantity(int itemId, [FromBody] UpdateQuantityDto dto)

    {
      if (dto.Quantity < 1)
      {
        return this.BadRequest(new { message = "Quantity must be at least 1." });
      }

      var item = await this._context.CartItems.FindAsync(itemId).ConfigureAwait(true);
      if (item == null)
      {
        return this.NotFound(new { message = "Item not found." });
      }

      item.Quantity = dto.Quantity;
      await this._context.SaveChangesAsync().ConfigureAwait(true);

      return this.Ok(new { message = "Quantity updated." });
    }

    //cart/item/{itemId}
    [HttpDelete("item/{cartItemId}")]
    [Authorize]
    public async Task<IActionResult> RemoveItem(int cartItemId)
    {
      var cartItem = await this._context.CartItems
          .Include(ci => ci.Product)
          .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId).ConfigureAwait(true);

      if (cartItem == null)
      {
        return this.NotFound(new { message = "Cart item not found." });
      }

      // Restore product stock
      if (cartItem.Product != null)
      {
        cartItem.Product.Quantity += cartItem.Quantity;
      }

      this._context.CartItems.Remove(cartItem);
      await this._context.SaveChangesAsync().ConfigureAwait(true);

      return this.Ok(new { message = "Item removed and stock restored." });
    }

    [Authorize]
    [HttpPost("expire")]
    public async Task<IActionResult> ExpireCart()
    {
      var userId = int.Parse(this.User.FindFirstValue(ClaimTypes.NameIdentifier));

      var cart = await this._context.Carts
          .Include(c => c.CartItems)
          .ThenInclude(i => i.Product)
          .FirstOrDefaultAsync(c => c.UserId == userId).ConfigureAwait(true);

      if (cart == null || !cart.CartItems.Any())
      {
        return this.Ok(new { message = "Cart already empty." });
      }

      foreach (var item in cart.CartItems)
      {
        item.Product.Quantity += item.Quantity; // restore stock
      }

      cart.CartItems.Clear();
      await this._context.SaveChangesAsync().ConfigureAwait(true);

      return this.Ok(new { message = "Cart expired. Items returned to stock." });
    }
  }
}