namespace KioskAPI.Controllers
{
  using KioskAPI.Data;
  using KioskAPI.Dtos;
  using KioskAPI.Mappers;
  using KioskAPI.Models;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.EntityFrameworkCore;

  [ApiController]
  [Route("api/[controller]")]
  public class OrderItemController : ControllerBase
  {
    private readonly AppDbContext _context;
    private const int CART_TIMEOUT_HOURS = 10;

    public OrderItemController(AppDbContext context)
    {
      this._context = context;
    }

    // Helper: Remove expired items AND restore stock
    private async Task RemoveExpiredItems(int orderId)
    {
      var expiredItems = await this._context.OrderItems
          .Where(i => i.OrderId == orderId && i.ExpiresAt < DateTime.UtcNow)
          .ToListAsync().ConfigureAwait(true);

      if (!expiredItems.Any())
      {
        return;
      }

      foreach (var item in expiredItems)
      {
        var product = await this._context.Products.FindAsync(item.ProductId).ConfigureAwait(true);

        if (product != null)
        {
          // Restore stock
          product.Quantity += item.Quantity;
        }
      }

      // Remove expired items
      this._context.OrderItems.RemoveRange(expiredItems);

      // Update order total
      var order = await this._context.Orders
          .Include(o => o.OrderItems)
          .FirstOrDefaultAsync(o => o.OrderId == orderId).ConfigureAwait(true);

      if (order != null)
      {
        order.TotalAmount = order.OrderItems
            .Where(i => i.ExpiresAt > DateTime.UtcNow)
            .Sum(i => i.Quantity * i.PriceAtPurchase);
      }

      await this._context.SaveChangesAsync().ConfigureAwait(true);
    }

    // GET all order items (admin)
    [HttpGet]
    public async Task<IActionResult> GetAllOrderItems()
    {
      var items = await this._context.OrderItems
          .Include(oi => oi.Product)
          .ToListAsync().ConfigureAwait(true);

      return this.Ok(items.Select(OrderItemMapper.ToDto).ToList());
    }

    // GET items for a specific order
    [HttpGet("order/{orderId}")]
    public async Task<IActionResult> GetItemsByOrder(int orderId)
    {
      // Auto-delete expired items
      await this.RemoveExpiredItems(orderId).ConfigureAwait(true);

      var items = await this._context.OrderItems
          .Include(oi => oi.Product)
          .Where(oi => oi.OrderId == orderId)
          .ToListAsync().ConfigureAwait(true);

      if (!items.Any())
      {
        return this.NotFound(new { message = "No items found for this order." });
      }

      var itemsDto = items.Select(OrderItemMapper.ToDto).ToList();

      return this.Ok(new
      {
        OrderId = orderId,
        Items = itemsDto,
        OrderTotal = itemsDto.Sum(i => i.Total)
      });
    }

    // POST add item to order
    [HttpPost("{orderId}")]
    public async Task<IActionResult> AddOrderItem(int orderId, [FromBody] CreateOrderItemDto newItemDto)
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

      var product = await this._context.Products.FindAsync(newItemDto.ProductId).ConfigureAwait(true);

      if (product == null)
      {
        return this.NotFound(new { message = "Product not found." });
      }

      if (product.Quantity < newItemDto.Quantity)
      {
        return this.BadRequest(new { message = $"Not enough stock for {product.Name}." });
      }

      // Reduce inventory
      product.Quantity -= newItemDto.Quantity;

      var orderItem = OrderItemMapper.ToEntity(newItemDto);

      orderItem.OrderId = orderId;
      orderItem.PriceAtPurchase = product.Price;
      orderItem.AddedAt = DateTime.UtcNow;
      orderItem.ExpiresAt = DateTime.UtcNow.AddMinutes(CART_TIMEOUT_HOURS);

      this._context.OrderItems.Add(orderItem);

      // Update order total
      order.TotalAmount = order.OrderItems.Sum(i => i.Quantity * i.PriceAtPurchase)
                         + (newItemDto.Quantity * product.Price);

      await this._context.SaveChangesAsync().ConfigureAwait(true);

      return this.Ok(new
      {
        message = "Item added successfully.",
        item = OrderItemMapper.ToDto(orderItem),
        order.TotalAmount
      });
    }

    // DELETE order item (admin)
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
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

      if (order != null)
      {
        order.TotalAmount = order.OrderItems
            .Where(i => i.OrderItemId != id)
            .Sum(i => i.Quantity * i.PriceAtPurchase);
      }

      await this._context.SaveChangesAsync().ConfigureAwait(true);

      return this.Ok(new { message = "Order item deleted, stock restored, and total updated." });
    }
  }
}
