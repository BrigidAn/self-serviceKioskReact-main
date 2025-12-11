namespace KioskAPI.Controllers
{
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.EntityFrameworkCore;
  using KioskAPI.Data;
  using KioskAPI.Dtos;
  using KioskAPI.Models;
  using System.Security.Claims;

  [ApiController]
  [Route("api/[controller]")]
  [Authorize]
  public class CheckoutController : ControllerBase
  {
    private readonly AppDbContext _context;

    public CheckoutController(AppDbContext context)
    {
      this._context = context;
    }

    private int GetIdentityUserId()
    {
      var claim = this.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
      return claim != null ? int.Parse(claim.Value) : 0;
    }

    // ------------------- SUMMARY -------------------
    [HttpGet("summary")]
    public async Task<IActionResult> GetCheckoutSummary()
    {
      int userId = this.GetIdentityUserId();
      if (userId == 0)
      {
        return this.Unauthorized(new { message = "Invalid token" });
      }

      var cart = await this._context.Carts
          .Include(c => c.CartItems)
          .ThenInclude(ci => ci.Product)
          .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsCheckedOut).ConfigureAwait(true);

      if (cart == null || !cart.CartItems.Any())
      {
        return this.NotFound(new { message = "Cart is empty" });
      }

      decimal itemsTotal = cart.CartItems.Sum(ci => ci.UnitPrice * ci.Quantity);

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

    // ------------------- CHECKOUT -------------------
    [HttpPost]
    public async Task<IActionResult> Checkout([FromBody] CheckoutRequestDto dto)
    {
      int userId;

      // Admin acting on behalf of a user
      if (this.User.IsInRole("Admin") && dto.UserId.HasValue)
      {
        userId = dto.UserId.Value;
      }
      else
      {
        userId = this.GetIdentityUserId();
      }

      if (userId == 0)
      {
        return this.Unauthorized(new { message = "Invalid token" });
      }

      var cart = await this._context.Carts
          .Include(c => c.CartItems)
          .ThenInclude(ci => ci.Product)
          .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsCheckedOut)
          .ConfigureAwait(true);

      if (cart == null || !cart.CartItems.Any())
      {
        return this.BadRequest(new { message = "Cart is empty" });
      }

      if (cart.ExpiresAt < DateTime.UtcNow)
      {
        return this.BadRequest(new { message = "Cart expired. Add items again." });
      }

      decimal itemsTotal = cart.CartItems.Sum(ci => ci.UnitPrice * ci.Quantity);
      decimal deliveryFee = dto.DeliveryMethod?.ToLower() == "delivery" ? 80 : 0;
      decimal grandTotal = itemsTotal + deliveryFee;

      var account = await this._context.Accounts.FirstOrDefaultAsync(a => a.UserId == userId).ConfigureAwait(true);
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

      // BEGIN TRANSACTION
      using var transaction = await this._context.Database.BeginTransactionAsync().ConfigureAwait(true);

      try
      {
        // Deduct account balance
        account.Balance -= grandTotal;

        // Create order
        var order = new Order
        {
          UserId = userId,
          DeliveryMethod = dto.DeliveryMethod,
          DeliveryFee = deliveryFee,
          TotalAmount = grandTotal,
          Status = "Pending",
          PaymentStatus = "Paid",
          OrderItems = new List<OrderItem>()
        };

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

        // Transactions log
        this._context.Transactions.Add(new Transaction
        {
          AccountId = account.AccountId,
          Type = "Checkout",
          TotalAmount = grandTotal,
          Description = $"Checkout: Items={itemsTotal}, DeliveryFee={deliveryFee}",
          CreatedAt = DateTime.UtcNow
        });

        // Mark cart as checked out & clear items
        cart.IsCheckedOut = true;
        this._context.CartItems.RemoveRange(cart.CartItems);

        await this._context.SaveChangesAsync().ConfigureAwait(true);
        await transaction.CommitAsync().ConfigureAwait(true);

        return this.Ok(new CheckoutResponseDto
        {
          CheckoutId = order.OrderId,
          DeliveryFee = deliveryFee,
          TotalAmount = grandTotal,
          DeliveryMethod = dto.DeliveryMethod,
          Message = "Checkout successful"
        });
      }
      catch (Exception ex)
      {
        await transaction.RollbackAsync().ConfigureAwait(true);
        return this.StatusCode(500, new { message = "Checkout failed", error = ex.Message });
      }
    }
  }
}
