namespace KioskAPI.Controllers
{
  using KioskAPI.Data;
  using KioskAPI.Dtos;
  using KioskAPI.Mappers;
  using KioskAPI.Models;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.EntityFrameworkCore;
  using System.Security.Claims;

  [ApiController]
  [Route("api/[controller]")]
  [Authorize] // 🔐 All endpoints now require JWT
  public class OrderItemController : ControllerBase
  {
    private readonly AppDbContext _context;

    public OrderItemController(AppDbContext context)
    {
      this._context = context;
    }

    // Helper: Get userId from JWT
    private int GetUserIdFromToken()
    {
      var id = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
      return id != null ? int.Parse(id) : 0;
    }

    // GET ALL order items (ADMIN ONLY)
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllOrderItems()
    {
      var items = await this._context.OrderItems
          .Include(oi => oi.Product)
          .ToListAsync().ConfigureAwait(true);

      return this.Ok(items.Select(OrderItemMapper.ToDto));
    }

    // GET items for a SPECIFIC ORDER
    // Only Admin OR the owner of the order can access
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

      // 🔐 Ensure user owns the order OR is Admin
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

    // ADD order item to an existing order
    // Only Admin OR Order Owner
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

      // 🔐 Only order owner or Admin may modify
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

      // Deduct stock
      product.Quantity -= dto.Quantity;

      var orderItem = OrderItemMapper.ToEntity(dto);
      orderItem.OrderId = orderId;
      orderItem.PriceAtPurchase = product.Price;

      this._context.OrderItems.Add(orderItem);

      // Update order total
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

    // DELETE item from order (Admin only)
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteOrderItem(int id)
    {
      var orderItem = await this._context.OrderItems.FindAsync(id).ConfigureAwait(true);
      if (orderItem == null)
      {
        return this.NotFound(new { message = "Order item not found." });
      }

      // Restore stock
      var product = await this._context.Products.FindAsync(orderItem.ProductId).ConfigureAwait(true);
      if (product != null)
      {
        product.Quantity += orderItem.Quantity;
      }

      var order = await this._context.Orders
          .Include(o => o.OrderItems)
          .FirstOrDefaultAsync(o => o.OrderId == orderItem.OrderId).ConfigureAwait(true);

      this._context.OrderItems.Remove(orderItem);

      // Recalculate order total
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
