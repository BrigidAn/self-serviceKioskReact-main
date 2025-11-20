using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KioskAPI.Data;
using KioskAPI.Models;
using KioskAPI.Dtos;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
  private readonly AppDbContext _context;
  private readonly IMapper _mapper;

  public OrderController(AppDbContext context, IMapper mapper)
  {
    this._context = context;
    this._mapper = mapper;
  }

  // GET all orders
  [HttpGet]
  public async Task<IActionResult> GetAllOrders()
  {
    var orders = await this._context.Orders
        .Include(o => o.User)
        .Include(o => o.OrderItems).ThenInclude(i => i.Product)
        .ToListAsync().ConfigureAwait(true);

    return this.Ok(this._mapper.Map<List<OrderDto>>(orders));
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

    return this.Ok(this._mapper.Map<List<OrderDto>>(orders));
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
      order = this._mapper.Map<OrderDto>(order)
    });
  }
}
