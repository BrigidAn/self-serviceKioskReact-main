namespace KioskAPI.Controllers
{
  using KioskAPI.Data;
  using KioskAPI.Dtos;
  using KioskAPI.Mappers;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.EntityFrameworkCore;
  using System.Security.Claims;

  /// <summary>
  /// Manages individual order items within orders in the self-service kiosk system.
  /// Handles viewing, adding, and removing items while enforcing stock and access rules.
  /// </summary>
  [ApiController]
  [Route("api/[controller]")]
  [Authorize]
  public class OrderItemController : ControllerBase
  {
    private readonly AppDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderItemController"/>.
    /// </summary>
    /// <param name="context">Injected database context.</param>
    public OrderItemController(AppDbContext context)
    {
      this._context = context;
    }

    /// <summary>
    /// Extracts the authenticated user's ID from the JWT token.
    /// </summary>
    /// <returns>
    /// The authenticated user's ID, or 0 if not available.
    /// </returns>
    private int GetUserIdFromToken()
    {
      var id = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
      return id != null ? int.Parse(id) : 0;
    }

    /// <summary>
    /// Retrieves all order items in the system.
    /// Admin-only endpoint used for audits and reporting.
    /// </summary>
    /// <returns>
    /// 200 OK with a list of all order items.
    /// </returns>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllOrderItems()
    {
      var items = await this._context.OrderItems
          .Include(oi => oi.Product)
          .ToListAsync().ConfigureAwait(true);

      return this.Ok(items.Select(OrderItemMapper.ToDto));
    }

    /// <summary>
    /// Retrieves all order items in the system.
    /// Admin-only endpoint used for audits and reporting.
    /// </summary>
    /// <returns>
    /// 200 OK with a list of all order items.
    /// </returns>
    [HttpGet("order/{orderId}")]
    public async Task<IActionResult> GetItemsByOrder(int orderId)
    {
      var order = await this._context.Orders
          .Include(o => o.OrderItems)
          .ThenInclude(oi => oi.Product)
          .FirstOrDefaultAsync(o => o.OrderId == orderId).ConfigureAwait(true);

      if (order == null)
      {
        return this.NotFound(new { message = "Order not found." });
      }

      var currentUserId = this.GetUserIdFromToken();
      var isAdmin = this.User.IsInRole("Admin");

      if (order.UserId != currentUserId && !isAdmin)
      {
        return this.Unauthorized(new { message = "You do not have access to this order." });
      }

      var itemsDto = order.OrderItems.Select(OrderItemMapper.ToDto).ToList();

      return this.Ok(new
      {
        OrderId = orderId,
        Items = itemsDto,
        OrderTotal = itemsDto.Sum(i => i.Total)
      });
    }

    /// <summary>
    /// Adds a new item to an existing order.
    /// Validates stock availability and order ownership.
    /// </summary>
    /// <param name="orderId">The order identifier.</param>
    /// <param name="dto">Order item creation data.</param>
    /// <returns>
    /// 200 OK if item is added successfully,
    /// 400 Bad Request for validation or stock issues,
    /// 401 Unauthorized if access is denied,
    /// 404 Not Found if order or product does not exist.
    /// </returns>
    [HttpPost("{orderId}")]
    public async Task<IActionResult> AddOrderItem(int orderId, [FromBody] CreateOrderItemDto dto)
    {
      if (!this.ModelState.IsValid)
      {
        return this.BadRequest(this.ModelState);
      }

      var order = await this._context.Orders
          .Include(o => o.OrderItems)
          .FirstOrDefaultAsync(o => o.OrderId == orderId).ConfigureAwait(true);

      if (order == null)
      {
        return this.NotFound(new { message = "Order not found." });
      }

      var currentUserId = this.GetUserIdFromToken();
      var isAdmin = this.User.IsInRole("Admin");

      if (order.UserId != currentUserId && !isAdmin)
      {
        return this.Unauthorized(new { message = "Not allowed to modify this order." });
      }

      var product = await this._context.Products.FindAsync(dto.ProductId).ConfigureAwait(true);
      if (product == null)
      {
        return this.NotFound(new { message = "Product not found." });
      }

      if (product.Quantity < dto.Quantity)
      {
        return this.BadRequest(new { message = $"Not enough stock for {product.Name}." });
      }

      product.Quantity -= dto.Quantity;

      var orderItem = OrderItemMapper.ToEntity(dto);
      orderItem.OrderId = orderId;
      orderItem.PriceAtPurchase = product.Price;

      this._context.OrderItems.Add(orderItem);

      order.TotalAmount = order.OrderItems
          .Sum(i => i.Quantity * i.PriceAtPurchase)
          + (dto.Quantity * product.Price);

      await this._context.SaveChangesAsync().ConfigureAwait(true);

      return this.Ok(new
      {
        message = "Item added to order.",
        item = OrderItemMapper.ToDto(orderItem),
        order.TotalAmount
      });
    }

    /// <summary>
    /// Deletes an order item and restores the associated product stock.
    /// Admin-only operation.
    /// </summary>
    /// <param name="id">The order item identifier.</param>
    /// <returns>
    /// 200 OK if item is deleted,
    /// 404 Not Found if item does not exist.
    /// </returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteOrderItem(int id)
    {
      var orderItem = await this._context.OrderItems.FindAsync(id).ConfigureAwait(true);
      if (orderItem == null)
      {
        return this.NotFound(new { message = "Order item not found." });
      }

      var product = await this._context.Products.FindAsync(orderItem.ProductId).ConfigureAwait(true);
      if (product != null)
      {
        product.Quantity += orderItem.Quantity;
      }

      var order = await this._context.Orders
          .Include(o => o.OrderItems)
          .FirstOrDefaultAsync(o => o.OrderId == orderItem.OrderId).ConfigureAwait(true);

      this._context.OrderItems.Remove(orderItem);

      if (order != null)
      {
        order.TotalAmount = order.OrderItems
            .Where(i => i.OrderItemId != id)
            .Sum(i => i.Quantity * i.PriceAtPurchase);
      }

      await this._context.SaveChangesAsync().ConfigureAwait(true);

      return this.Ok(new { message = "Order item deleted and stock restored." });
    }
  }
}
