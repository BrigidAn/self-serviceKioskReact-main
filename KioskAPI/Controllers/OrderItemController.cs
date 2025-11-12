using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KioskAPI.Data;
using KioskAPI.Models;

namespace KioskAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderItemController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrderItemController(AppDbContext context)
        {
            _context = context;
        }

        // Get all order items (for admin or reports)
        [HttpGet]
        public async Task<IActionResult> GetAllOrderItems()
        {
            var items = await _context.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.Product)
                .Select(oi => new
                {
                    oi.OrderItemId,
                    oi.OrderId,
                    oi.ProductId,
                    ProductName = oi.Product.Name,
                    oi.Quantity,
                    oi.PriceAtPurchase,
                    Total = oi.Quantity * oi.PriceAtPurchase
                })
                .ToListAsync();

            return Ok(items);
        }

        // Get all items belonging to a specific order
        [HttpGet("order/{orderId}")]
        public async Task<IActionResult> GetItemsByOrder(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
                return NotFound(new { message = "Order not found." });

            var items = order.OrderItems.Select(oi => new
            {
                oi.OrderItemId,
                oi.ProductId,
                ProductName = oi.Product.Name,
                oi.Quantity,
                oi.PriceAtPurchase,
                Total = oi.Quantity * oi.PriceAtPurchase
            });

            return Ok(new
            {
                OrderId = order.OrderId,
                OrderDate = order.OrderDate,
                Items = items,
                OrderTotal = items.Sum(i => i.Total)
            });
        }

        // Add an item to an existing order
        [HttpPost]
        public async Task<IActionResult> AddOrderItem([FromBody] OrderItem newItem)
        {
            if (newItem == null)
                return BadRequest(new { message = "Invalid order item data." });

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderId == newItem.OrderId);

            if (order == null)
                return NotFound(new { message = "Order not found." });

            var product = await _context.Products.FindAsync(newItem.ProductId);
            if (product == null)
                return NotFound(new { message = "Product not found." });

            // ✅ Set default price from product if not provided
            if (newItem.PriceAtPurchase <= 0)
                newItem.PriceAtPurchase = product.Price;

            // ✅ Add item to the order
            _context.OrderItems.Add(newItem);
            await _context.SaveChangesAsync();

            // ✅ Update order total
            order.TotalAmount = order.OrderItems.Sum(i => i.Quantity * i.PriceAtPurchase);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Item added successfully.",
                newItem.OrderItemId,
                order.TotalAmount
            });
        }

        // Update item quantity or price
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrderItem(int id, [FromBody] OrderItem updatedItem)
        {
            var existingItem = await _context.OrderItems.FindAsync(id);
            if (existingItem == null)
                return NotFound(new { message = "Order item not found." });

            existingItem.Quantity = updatedItem.Quantity > 0 ? updatedItem.Quantity : existingItem.Quantity;
            existingItem.PriceAtPurchase = updatedItem.PriceAtPurchase > 0 ? updatedItem.PriceAtPurchase : existingItem.PriceAtPurchase;

            await _context.SaveChangesAsync();

            // Recalculate order total
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderId == existingItem.OrderId);

            if (order != null)
            {
                order.TotalAmount = order.OrderItems.Sum(i => i.Quantity * i.PriceAtPurchase);
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Order item updated successfully." });
        }

        // ❌ DELETE: api/orderitem/{id}
        // Remove an item from an order
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
