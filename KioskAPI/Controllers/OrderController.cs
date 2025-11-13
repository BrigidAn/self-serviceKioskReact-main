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
    public class OrderController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrderController(AppDbContext context)
        {
            _context = context;
        }

   
        // Get all orders (admin only, optional)
        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(i => i.Product)
                .Select(o => new
                {
                    o.OrderId,
                    Customer = o.User.Name,
                    o.OrderDate,
                    o.TotalAmount,
                    o.Status,
                    o.PaymentStatus,
                    Items = o.OrderItems.Select(i => new
                    {
                        i.ProductId,
                        ProductName = i.Product.Name,
                        i.Quantity,
                        i.PriceAtPurchase
                    })
                })
                .ToListAsync();

            return Ok(orders);
        }

        // Get orders belonging to a specific user
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetOrdersByUser(int userId)
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(i => i.Product)
                .Where(o => o.UserId == userId)
                .Select(o => new
                {
                    o.OrderId,
                    o.OrderDate,
                    o.TotalAmount,
                    o.Status,
                    o.PaymentStatus,
                    Items = o.OrderItems.Select(i => new
                    {
                        i.ProductId,
                        ProductName = i.Product.Name,
                        i.Quantity,
                        i.PriceAtPurchase
                    })
                })
                .ToListAsync();

            if (orders.Count == 0)
                return NotFound(new { message = "No orders found for this user." });

            return Ok(orders);
        }

        
        // Create a new order
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] Order newOrder)
        {
            if (newOrder == null || newOrder.OrderItems == null || !newOrder.OrderItems.Any())
                return BadRequest(new { message = "Order must contain at least one item." });

            // Calculate total amount
            decimal totalAmount = 0;
            foreach (var item in newOrder.OrderItems)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product == null)
                    return BadRequest(new { message = $"Product ID {item.ProductId} not found." });

                if (product.Quantity < item.Quantity)
                    return BadRequest(new { message = $"Not enough stock for product {product.Name}." });

                // Deduct stock
                product.Quantity -= item.Quantity;

                // Use current product price as purchase price
                item.PriceAtPurchase = product.Price;
                totalAmount += product.Price * item.Quantity;
            }

            newOrder.OrderDate = DateTime.UtcNow;
            newOrder.TotalAmount = totalAmount;
            newOrder.Status = "Pending";
            newOrder.PaymentStatus = "Unpaid";

            _context.Orders.Add(newOrder);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Order created successfully.",
                newOrder.OrderId,
                newOrder.TotalAmount
            });
        }

        
        // Update order status (e.g., mark as completed or paid)
        [HttpPut("{orderId}")]
        public async Task<IActionResult> UpdateOrder(int orderId, [FromBody] Order updatedOrder)
        {
            var existingOrder = await _context.Orders.FindAsync(orderId);

            if (existingOrder == null)
                return NotFound(new { message = "Order not found." });

            existingOrder.Status = updatedOrder.Status ?? existingOrder.Status;
            existingOrder.PaymentStatus = updatedOrder.PaymentStatus ?? existingOrder.PaymentStatus;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Order updated successfully." });
        }

        // ❌ DELETE: api/order/{orderId}
        [HttpDelete("{orderId}")]
        public async Task<IActionResult> DeleteOrder(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
                return NotFound(new { message = "Order not found." });

            // Return products to stock before deleting order
            foreach (var item in order.OrderItems)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                    product.Quantity += item.Quantity;
            }

            _context.OrderItems.RemoveRange(order.OrderItems);
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Order deleted successfully." });
        }
    }
}
