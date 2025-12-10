namespace KioskAPI.Controllers
{
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.EntityFrameworkCore;
  using KioskAPI.Data;
  using KioskAPI.Models;
  using KioskAPI.Dtos;
  using Microsoft.AspNetCore.Authorization;
  using KioskAPI.Mappers;
  using Microsoft.Extensions.Configuration.UserSecrets;
  using System.Security.Claims;

  [ApiController]
  [Route("api/[controller]")]
  public class OrderController : ControllerBase
  {
    private readonly AppDbContext _context;

    public OrderController(AppDbContext context)
    {
      this._context = context;
    }

    // GET all orders
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAllOrders()
    {
      var orders = await this._context.Orders
          .Include(o => o.User)
          .Include(o => o.OrderItems).ThenInclude(i => i.Product)
          .ToListAsync().ConfigureAwait(true);

      return this.Ok(orders.Select(OrderMapper.ToDto));
    }

    // GET orders for a user
    [Authorize(Roles = "Admin")]
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetOrdersByUser(int userId)
    {
      var orders = await this._context.Orders
          .Include(o => o.OrderItems).ThenInclude(i => i.Product)
          .Where(o => o.UserId == userId)
          .ToListAsync().ConfigureAwait(true);

      if (!orders.Any())
      {
        return this.NotFound(new { message = "No orders found for this user." });
      }

      return this.Ok(orders.Select(OrderMapper.ToDto));
    }

    //logged-in user's own orders
    [Authorize]
    [HttpGet("myOrders")]
    public async Task<IActionResult> GetMyOrders()
    {
      var userId = int.Parse(this.User.FindFirstValue(ClaimTypes.NameIdentifier));

      var orders = await this._context.Orders
          .Include(o => o.OrderItems)
              .ThenInclude(i => i.Product)
          .Where(o => o.UserId == userId)
          .ToListAsync().ConfigureAwait(true);

      return this.Ok(orders.Select(OrderMapper.ToDto));
    }

    // POST create new order
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto newOrderDto)
    {
      if (!this.ModelState.IsValid)
      {
        return this.BadRequest(this.ModelState);
      }

      var order = new Order
      {
        UserId = newOrderDto.UserId,
        OrderDate = DateTime.UtcNow,
        Status = "Pending",
        PaymentStatus = "Unpaid",
        OrderItems = new List<OrderItem>()
      };

      decimal totalAmount = 0;

      foreach (var itemDto in newOrderDto.Items)
      {
        var product = await this._context.Products.FindAsync(itemDto.ProductId).ConfigureAwait(true);
        if (product == null)
        {
          return this.BadRequest(new { message = $"Product ID {itemDto.ProductId} not found." });
        }

        if (product.Quantity < itemDto.Quantity)
        {
          return this.BadRequest(new { message = $"Not enough stock for {product.Name}." });
        }

        // Stock update
        product.Quantity -= itemDto.Quantity;

        var orderItem = new OrderItem
        {
          ProductId = itemDto.ProductId,
          Quantity = itemDto.Quantity,
          PriceAtPurchase = product.Price
        };

        order.OrderItems.Add(orderItem);
        totalAmount += product.Price * itemDto.Quantity;
      }

      order.TotalAmount = totalAmount;

      this._context.Orders.Add(order);
      await this._context.SaveChangesAsync().ConfigureAwait(true);

      return this.Ok(new
      {
        message = "Order created successfully",
        order = OrderMapper.ToDto(order)
      });
    }
    // PUT: api/order/{orderId}/complete
    [Authorize]
    [HttpPost("complete/{orderId}")]
    public async Task<IActionResult> CompleteOrder(int orderId)
    {
      var userId = int.Parse(this.User.FindFirstValue(ClaimTypes.NameIdentifier));

      var order = await this._context.Orders
          .FirstOrDefaultAsync(o => o.OrderId == orderId && o.UserId == userId).ConfigureAwait(true);

      if (order == null)
      {
        return this.NotFound(new { message = "Order not found" });
      }

      order.Status = "Complete";
      order.PaymentStatus = "Paid";

      await this._context.SaveChangesAsync().ConfigureAwait(true);
      return this.Ok(new { message = "Order marked as complete" });
    }
  }
}