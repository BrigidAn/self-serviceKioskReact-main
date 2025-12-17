namespace KioskAPI.Controllers
{
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.EntityFrameworkCore;
  using KioskAPI.Data;
  using KioskAPI.Models;
  using KioskAPI.Dtos;
  using Microsoft.AspNetCore.Authorization;
  using KioskAPI.Mappers;
  using System.Security.Claims;

  [ApiController]
  [Route("api/[controller]")]
  [Authorize]
  public class OrderController : ControllerBase
  {
    private readonly AppDbContext _context;
    private readonly ILogger<OrderController> _logger;

    public OrderController(AppDbContext context, ILogger<OrderController> logger)
    {
      this._context = context;
      this._logger = logger;
    }

    // GET all orders (Admin)
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAllOrders()
    {
      var orders = await this._context.Orders
          .Include(o => o.User)
          .Include(o => o.OrderItems)
            .ThenInclude(i => i.Product)
          .ToListAsync()
          .ConfigureAwait(true);

      return this.Ok(orders.Select(OrderMapper.ToDto));
    }

    // GET orders by user (Admin)
    [Authorize(Roles = "Admin")]
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetOrdersByUser(int userId)
    {
      var orders = await this._context.Orders
          .Include(o => o.OrderItems)
            .ThenInclude(i => i.Product)
          .Where(o => o.UserId == userId)
          .ToListAsync()
          .ConfigureAwait(true);

      if (!orders.Any())
      {
        this._logger.LogWarning(
          "No orders found for user {UserId} (admin request)",
          userId);

        return this.NotFound(new { message = "No orders found for this user." });
      }

      return this.Ok(orders.Select(OrderMapper.ToDto));
    }

    // GET logged-in user's orders
    [HttpGet("myOrders")]
    public async Task<IActionResult> GetMyOrders()
    {
      var userId = int.Parse(this.User.FindFirstValue(ClaimTypes.NameIdentifier));

      var orders = await this._context.Orders
          .Include(o => o.OrderItems)
            .ThenInclude(i => i.Product)
          .Where(o => o.UserId == userId)
          .ToListAsync()
          .ConfigureAwait(true);

      return this.Ok(orders.Select(OrderMapper.ToDto));
    }

    // POST create new order
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto newOrderDto)
    {
      if (!this.ModelState.IsValid)
      {
        this._logger.LogWarning(
          "CreateOrder failed: Invalid model state for user {UserId}",
          newOrderDto.UserId);

        return this.BadRequest(this.ModelState);
      }

      using var transaction = await this._context.Database.BeginTransactionAsync().ConfigureAwait(true);
      try
      {
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
          var product = await this._context.Products
            .FindAsync(itemDto.ProductId)
            .ConfigureAwait(true);

          if (product == null)
          {
            this._logger.LogWarning(
              "CreateOrder failed: Product {ProductId} not found for user {UserId}",
              itemDto.ProductId, newOrderDto.UserId);

            return this.BadRequest(new { message = $"Product ID {itemDto.ProductId} not found." });
          }

          if (product.Quantity < itemDto.Quantity)
          {
            this._logger.LogWarning(
              "CreateOrder failed: Insufficient stock for product {ProductId}. Requested {Quantity}, Available {Stock}. User {UserId}",
              product.ProductId, itemDto.Quantity, product.Quantity, newOrderDto.UserId);

            return this.BadRequest(new { message = $"Not enough stock for {product.Name}." });
          }

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

        await transaction.CommitAsync().ConfigureAwait(true);

        this._logger.LogInformation(
          "Order {OrderId} created for user {UserId}",
          order.OrderId, newOrderDto.UserId);

        return this.Ok(new
        {
          message = "Order created successfully",
          order = OrderMapper.ToDto(order)
        });
      }
      catch (Exception ex)
      {
        await transaction.RollbackAsync().ConfigureAwait(true);
        this._logger.LogError(
          ex, "Error creating order for user {UserId}",
          newOrderDto.UserId);

        return this.StatusCode(500, new { message = "Error creating order" });
      }
    }

    // POST complete order
    [HttpPost("complete/{orderId}")]
    public async Task<IActionResult> CompleteOrder(int orderId)
    {
      var userId = int.Parse(this.User.FindFirstValue(ClaimTypes.NameIdentifier));

      using var transaction = await this._context.Database.BeginTransactionAsync().ConfigureAwait(true);
      try
      {
        var order = await this._context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.OrderId == orderId && o.UserId == userId)
            .ConfigureAwait(true);

        if (order == null)
        {
          this._logger.LogWarning(
            "CompleteOrder failed: Order {OrderId} not found for user {UserId}",
            orderId, userId);

          return this.NotFound(new { message = "Order not found" });
        }

        order.Status = "Complete";
        order.PaymentStatus = "Paid";

        await this._context.SaveChangesAsync().ConfigureAwait(true);
        await transaction.CommitAsync().ConfigureAwait(true);

        this._logger.LogInformation(
          "Order {OrderId} completed for user {UserId}",
          orderId, userId);

        return this.Ok(new { message = "Order marked as complete" });
      }
      catch (Exception ex)
      {
        await transaction.RollbackAsync().ConfigureAwait(true);
        this._logger.LogError(
          ex, "Error completing order {OrderId} for user {UserId}",
          orderId, userId);

        return this.StatusCode(500, new { message = "Error completing order" });
      }
    }
  }
}
