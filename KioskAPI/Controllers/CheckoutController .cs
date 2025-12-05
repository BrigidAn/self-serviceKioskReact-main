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
      using var transaction = await this._context.Database.BeginTransactionAsync().ConfigureAwait(true);

      var cart = await this._context.Carts
          .Include(c => c.CartItems)
          .ThenInclude(ci => ci.Product)
          .FirstOrDefaultAsync(c => c.UserId == dto.UserId && !c.IsCheckedOut).ConfigureAwait(true);

      if (cart == null || !cart.CartItems.Any())
      {
        return this.BadRequest(new { message = "Cart is empty or not found" });
      }

      decimal itemsTotal = cart.CartItems.Sum(ci => ci.UnitPrice * ci.Quantity);
      decimal deliveryFee = dto.DeliveryMethod?.ToLower() == "delivery" ? 80 : 0;
      decimal grandTotal = itemsTotal + deliveryFee;

      var account = await this._context.Accounts
          .FirstOrDefaultAsync(a => a.UserId == dto.UserId).ConfigureAwait(true);

      if (account == null)
      {
        return this.BadRequest(new { message = "User account not found" });
      }

      if (account.Balance < grandTotal)
      {
        decimal remaining = grandTotal - account.Balance;
        decimal deducted = account.Balance;

        account.Balance = 0;
        await this._context.SaveChangesAsync().ConfigureAwait(true);

        return this.BadRequest(new
        {
          message = "Insufficient balance",
          remainingAmount = remaining,
          deductedAmount = deducted,
          itemsTotal,
          deliveryFee,
          grandTotal
        });
      }

      // Create order
      var order = new Order
      {
        UserId = dto.UserId,
        DeliveryFee = deliveryFee,
        TotalAmount = grandTotal,
        DeliveryMethod = dto.DeliveryMethod,
        Status = "Pending",
        PaymentStatus = "Paid",
        OrderItems = new List<OrderItem>()
      };

      // Deduct money from account
      account.Balance -= grandTotal;

      this._context.Transactions.Add(new Transaction
      {
        AccountId = account.AccountId,
        Type = "Checkout",
        TotalAmount = grandTotal,
        Description = $"Checkout: Items={itemsTotal}, DeliveryFee={deliveryFee}",
        CreatedAt = DateTime.UtcNow
      });

      // Create order items
      foreach (var item in cart.CartItems)
      {
        if (item.Product.Quantity < item.Quantity)
        {
          return this.BadRequest(new { message = $"Not enough stock for {item.Product.Name}" });
        }

        order.OrderItems.Add(new OrderItem
        {
          ProductId = item.ProductId,
          Quantity = item.Quantity,
          PriceAtPurchase = item.UnitPrice
        });

        item.Product.Quantity -= item.Quantity;
      }

      this._context.Orders.Add(order);

      // Mark cart as completed
      cart.IsCheckedOut = true;

      // clear the cart
      this._context.CartItems.RemoveRange(cart.CartItems);

      await this._context.SaveChangesAsync().ConfigureAwait(true);
      await transaction.CommitAsync().ConfigureAwait(true);

      return this.Ok(new CheckoutResponseDto
      {
        CheckoutId = order.OrderId,      // IMPORTANT FIX
        DeliveryFee = deliveryFee,
        TotalAmount = grandTotal,
        DeliveryMethod = dto.DeliveryMethod,
        Message = "Checkout successful"
      });
    }
  }
}
