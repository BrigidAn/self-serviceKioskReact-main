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
      int userId = this.GetIdentityUserId();
      if (userId == 0)
      {
        return this.Unauthorized();
      }

      var product = await this._context.Products.FindAsync(dto.ProductId).ConfigureAwait(true);
      if (product == null)
      {
        return this.BadRequest(new { message = "Product not found" });
      }

      if (product.Quantity < dto.Quantity)
      {
        return this.BadRequest(new { message = "Not enough stock" });
      }

      var cart = await this._context.Carts
          .Include(c => c.CartItems)
          .FirstOrDefaultAsync(c => c.UserId == dto.UserId && !c.IsCheckedOut).ConfigureAwait(true);

      if (cart == null)
      {
        cart = new Cart { UserId = dto.UserId };
        this._context.Carts.Add(cart);
        await this._context.SaveChangesAsync().ConfigureAwait(true);
      }

      // ========== CART EXPIRED? ==========
      if (cart != null && cart.ExpiresAt < DateTime.UtcNow)
      {
        this._context.CartItems.RemoveRange(cart.CartItems);
        this._context.Carts.Remove(cart);
        await this._context.SaveChangesAsync().ConfigureAwait(true);

        cart = null; // recreate below
      }

      if (cart == null)
      {
        cart = new Cart
        {
          UserId = userId,
          ExpiresAt = DateTime.UtcNow.AddMinutes(15) // NEW
        };
        this._context.Carts.Add(cart);
        await this._context.SaveChangesAsync().ConfigureAwait(true);
      }
      else
      {
        // Extend timer on every add
        cart.ExpiresAt = DateTime.UtcNow.AddMinutes(15);
      }

      var cartItem = new CartItem
      {
        CartId = cart.CartId,
        Cart = cart,
        ProductId = product.ProductId,
        Product = product,
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
    [HttpDelete("item/{itemId}")]
    public async Task<IActionResult> RemoveItem(int itemId)
    {
      var item = await this._context.CartItems.FindAsync(itemId).ConfigureAwait(true);
      if (item == null)
      {
        return this.NotFound(new { message = "Item not found." });
      }

      this._context.CartItems.Remove(item);
      await this._context.SaveChangesAsync().ConfigureAwait(true);

      return this.Ok(new { message = "Item removed." });
    }
  }
}