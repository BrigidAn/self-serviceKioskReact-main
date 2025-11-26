namespace KioskAPI.Controllers
{
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.EntityFrameworkCore;
  using KioskAPI.Data;
  using KioskAPI.Dtos;
  using KioskAPI.Models;

  [ApiController]
  [Route("api/[controller]")]
  public class CheckoutController : ControllerBase
  {
    private readonly AppDbContext _context;

    public CheckoutController(AppDbContext context)
    {
      this._context = context;
    }

    [HttpPost]
    public async Task<IActionResult> Checkout([FromBody] CheckoutRequestDto dto)
    {
      var cart = await this._context.Carts
          .Include(c => c.CartItems)
          .ThenInclude(ci => ci.Product)
          .FirstOrDefaultAsync(c => c.CartId == dto.CartId && c.UserId == dto.UserId && !c.IsCheckedOut).ConfigureAwait(true);

      if (cart == null || !cart.CartItems.Any())
      {
        return this.BadRequest(new { message = "Cart is empty or not found" });
      }

      decimal total = cart.CartItems.Sum(ci => ci.UnitPrice * ci.Quantity);

      // Optional: validate user balance here if needed
      // var user = await _context.Users.FindAsync(dto.UserId);
      // if (user.Balance < total) return BadRequest("Insufficient balance");

      cart.IsCheckedOut = true;

      await this._context.SaveChangesAsync().ConfigureAwait(true);

      return this.Ok(new CheckoutResponseDto
      {
        CheckoutId = cart.CartId,
        TotalAmount = total,
        DeliveryMethod = dto.DeliveryMethod,
        Message = "Checkout successful"
      });
    }
  }
}
