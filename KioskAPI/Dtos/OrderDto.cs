using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KioskAPI.Models;

namespace KioskAPI.Dtos
{
    public class OrderDto
    {
    public int OrderId { get; set; }
    public string? CustomerName { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Status { get; set; } // e.g., "Pending", "Completed"
    public string? PaymentStatus { get; set; } // e.g., "Paid", "Unpaid"
    public List<OrderItemDto>? Items { get; set; }

    }

    public class CreateOrderDto
    {
        public int UserId { get; set; }
        public List<CreateOrderItemDto> Items { get; set; } = new();
    }
}