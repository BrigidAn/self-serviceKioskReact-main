using AutoMapper;
using KioskAPI.Data;
using KioskAPI.Dtos;
using KioskAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KioskAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderItemController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public OrderItemController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET all order items
        [HttpGet]
        public async Task<IActionResult> GetAllOrderItems()
        {
            var items = await _context.OrderItems
                .Include(oi => oi.Product)
                .ToListAsync();

            var itemsDto = _mapper.Map<List<OrderItemDto>>(items);
            return Ok(itemsDto);
        }

        // GET items by order
        [HttpGet("order/{orderId}")]
        public async Task<IActionResult> GetItemsByOrder(int orderId)
        {
            var items = await _context.OrderItems
                .Include(oi => oi.Product)
                .Where(oi => oi.OrderId == orderId)
                .ToListAsync();

            if (!items.Any())
                return NotFound(new { message = "No items found for this order." });

            var itemsDto = _mapper.Map<List<OrderItemDto>>(items);
            var orderTotal = itemsDto.Sum(i => i.Total);

            return Ok(new
            {
                OrderId = orderId,
                Items = itemsDto,
                OrderTotal = orderTotal
            });
        }

        // POST add order item
        [HttpPost]
        public async Task<IActionResult> AddOrderItem([FromBody] CreateOrderItemDto newItemDto)
        {
            if (newItemDto == null)
                return BadRequest(new { message = "Invalid order item data." });

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderId == newItemDto.OrderId);
            if (order == null)
                return NotFound(new { message = "Order not found." });

            var product = await _context.Products.FindAsync(newItemDto.ProductId);
            if (product == null)
                return NotFound(new { message = "Product not found." });

            var orderItem = _mapper.Map<OrderItem>(newItemDto);
            orderItem.PriceAtPurchase = product.Price;

            _context.OrderItems.Add(orderItem);
            await _context.SaveChangesAsync();

            // Update order total
            order.TotalAmount = order.OrderItems.Sum(i => i.Quantity * i.PriceAtPurchase);
            await _context.SaveChangesAsync();

            var orderItemDto = _mapper.Map<OrderItemDto>(orderItem);
            return Ok(new { message = "Item added successfully", orderItem = orderItemDto, order.TotalAmount });
        }

        // PUT update order item
        // [HttpPut("{id}")]
        // public async Task<IActionResult> UpdateOrderItem(int id, [FromBody] OrderItem updatedItem)
        // {
        //     var existingItem = await _context.OrderItems.FindAsync(id);
        //     if (existingItem == null)
        //         return NotFound(new { message = "Order item not found." });

        //     existingItem.Quantity = updatedItem.Quantity > 0 ? updatedItem.Quantity : existingItem.Quantity;
        //     existingItem.PriceAtPurchase = updatedItem.PriceAtPurchase > 0 ? updatedItem.PriceAtPurchase : existingItem.PriceAtPurchase;

        //     await _context.SaveChangesAsync();

        //     // Recalculate order total
        //     var order = await _context.Orders
        //         .Include(o => o.OrderItems)
        //         .FirstOrDefaultAsync(o => o.OrderId == existingItem.OrderId);

        //     if (order != null)
        //     {
        //         order.TotalAmount = order.OrderItems.Sum(i => i.Quantity * i.PriceAtPurchase);
        //         await _context.SaveChangesAsync();
        //     }

        //     var orderItemDto = _mapper.Map<OrderItemDto>(existingItem);
        //     return Ok(new { message = "Order item updated successfully", orderItem = orderItemDto, order.TotalAmount });
        // }

        // DELETE order item
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrderItem(int id)
        {
            var orderItem = await _context.OrderItems.FindAsync(id);
            if (orderItem == null)
                return NotFound(new { message = "Order item not found." });

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderId == orderItem.OrderId);

            _context.OrderItems.Remove(orderItem);
            await _context.SaveChangesAsync();

            // Recalculate total
            if (order != null)
            {
                order.TotalAmount = order.OrderItems.Sum(i => i.Quantity * i.PriceAtPurchase);
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Order item deleted and order total updated." });
        }
    }
}
