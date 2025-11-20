namespace KioskAPI.Controllers
{
  using AutoMapper;
  using KioskAPI.Data;
  using KioskAPI.Dtos;
  using KioskAPI.Models;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.EntityFrameworkCore;

  [ApiController]
  [Route("api/[controller]")]
  public class OrderItemController : ControllerBase
  {
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public OrderItemController(AppDbContext context, IMapper mapper)
    {
      this._context = context;
      this._mapper = mapper;
    }

    // GET all order items (mainly admin/debug)
    [HttpGet]
    public async Task<IActionResult> GetAllOrderItems()
    {
      var items = await this._context.OrderItems
          .Include(oi => oi.Product)
          .ToListAsync().ConfigureAwait(true);

      var itemsDto = this._mapper.Map<List<OrderItemDto>>(items);
      return this.Ok(itemsDto);
    }

    // GET items for a specific order
    [HttpGet("order/{orderId}")]
    public async Task<IActionResult> GetItemsByOrder(int orderId)
    {
      var items = await this._context.OrderItems
          .Include(oi => oi.Product)
          .Where(oi => oi.OrderId == orderId)
          .ToListAsync().ConfigureAwait(true);

      if (!items.Any())
      {
        return this.NotFound(new { message = "No items found for this order." });
      }

      var itemsDto = this._mapper.Map<List<OrderItemDto>>(items);

      return this.Ok(new
      {
        OrderId = orderId,
        Items = itemsDto,
        OrderTotal = itemsDto.Sum(i => i.Total)
      });
    }

    // POST add item to an order
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

      // update inventory
      product.Quantity -= newItemDto.Quantity;

      var orderItem = this._mapper.Map<OrderItem>(newItemDto);
      orderItem.OrderId = orderId;
      orderItem.PriceAtPurchase = product.Price;

      this._context.OrderItems.Add(orderItem);

      // recalc total
      order.TotalAmount = order.OrderItems.Sum(i => i.Quantity * i.PriceAtPurchase)
                           + (newItemDto.Quantity * product.Price);

      await this._context.SaveChangesAsync().ConfigureAwait(true);

      return this.Ok(new
      {
        message = "Item added successfully.",
        item = this._mapper.Map<OrderItemDto>(orderItem),
        order.TotalAmount
      });
    }

    // DELETE order item
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrderItem(int id)
    {
      var orderItem = await this._context.OrderItems.FindAsync(id).ConfigureAwait(true);
      if (orderItem == null)
      {
        return this.NotFound(new { message = "Order item not found." });
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

      return this.Ok(new { message = "Order item deleted and order total updated." });
    }
  }
}
