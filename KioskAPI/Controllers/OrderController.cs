using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KioskAPI.Data;
using KioskAPI.Models;
using KioskAPI.Dtos;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public OrderController(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    // GET all orders (admin)
    [HttpGet]
    public async Task<IActionResult> GetAllOrders()
    {
        var orders = await _context.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems)
            .ThenInclude(i => i.Product)
            .ToListAsync();

        var ordersDto = _mapper.Map<List<OrderDto>>(orders);
        return Ok(ordersDto);
    }

    // GET orders by user
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetOrdersByUser(int userId)
    {
        var orders = await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(i => i.Product)
            .Include(o => o.User)
            .Where(o => o.UserId == userId)
            .ToListAsync();

        if (!orders.Any())
            return NotFound(new { message = "No orders found for this user." });

        var ordersDto = _mapper.Map<List<OrderDto>>(orders);
        return Ok(ordersDto);
    }

    // POST create order
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto newOrderDto)
    {
        if (newOrderDto == null || newOrderDto.Items == null || !newOrderDto.Items.Any())
            return BadRequest(new { message = "Order must contain at least one item." });

        var order = _mapper.Map<Order>(newOrderDto);
        order.OrderDate = DateTime.UtcNow;
        order.Status = "Pending";
        order.PaymentStatus = "Unpaid";
        order.OrderItems = new List<OrderItem>();

        decimal totalAmount = 0;

        foreach (var itemDto in newOrderDto.Items)
        {
            var product = await _context.Products.FindAsync(itemDto.ProductId);
            if (product == null)
                return BadRequest(new { message = $"Product ID {itemDto.ProductId} not found." });

            if (product.Quantity < itemDto.Quantity)
                return BadRequest(new { message = $"Not enough stock for product {product.Name}." });

            product.Quantity -= itemDto.Quantity;

            var orderItem = _mapper.Map<OrderItem>(itemDto);
            orderItem.PriceAtPurchase = product.Price;

            order.OrderItems.Add(orderItem);
            totalAmount += product.Price * itemDto.Quantity;
        }

        order.TotalAmount = totalAmount;
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        var orderDto = _mapper.Map<OrderDto>(order);
        return Ok(new { message = "Order created successfully", order = orderDto });
    }
}
