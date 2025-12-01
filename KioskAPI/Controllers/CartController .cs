namespace KioskAPI.Controllers
{
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.EntityFrameworkCore;
  using KioskAPI.Data;
  using KioskAPI.Dtos;
  using KioskAPI.Models;
  using Microsoft.AspNetCore.Authorization;
  using System.Security.Claims;
  using KioskAPI.interfaces;

  [ApiController]
  [Route("api/[controller]")]
  [Authorize] // 🔐 Require JWT
  public class CartController : ControllerBase
  {
    private readonly ICartRepository _cartRepo;

    public CartController(ICartRepository cartRepo)
    {
      this._cartRepo = cartRepo;
    }
    // 🔹 Get logged-in user's ID
    private int GetUserId()
    {
      var userIdString = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
      return int.Parse(userIdString);
    }

    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
      int userId = this.GetUserId();
      var cart = await this._cartRepo.GetUserCart(userId).ConfigureAwait(true);
      return this.Ok(cart);
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddItem([FromBody] AddToCartDto dto)
    {
      int userId = this.GetUserId();
      var item = await this._cartRepo.AddToCart(userId, dto.ProductId, dto.Quantity).ConfigureAwait(true);
      return Ok(item);
    }

    [HttpPut("update/{cartItemId}")]
    public async Task<IActionResult> UpdateItem(int cartItemId, [FromBody] UpdateQuantityDto dto)
    {
      int userId = this.GetUserId();
      bool updated = await this._cartRepo.UpdateQuantity(userId, cartItemId, dto.Quantity).ConfigureAwait(true);

      if (!updated)
      {
        return this.NotFound();
      }

      return this.Ok(new { message = "Updated successfully" });
    }

    [HttpDelete("remove/{cartItemId}")]
    public async Task<IActionResult> RemoveItem(int cartItemId)
    {
      int userId = this.GetUserId();
      bool removed = await this._cartRepo.RemoveCartItem(userId, cartItemId).ConfigureAwait(true);

      if (!removed)
      {
        return this.NotFound();
      }

      return this.Ok(new { message = "Item removed" });
    }
  }
}