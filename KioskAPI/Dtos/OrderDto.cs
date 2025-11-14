using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KioskAPI.Models;

namespace KioskAPI.Dtos
{
    public class OrderDto
    {
        public int UserId { get; set; }
        public User? User { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Status { get; set; } // e.g., "Pending", "Completed"
        public string? DeliveryMethod { get; set; }
        public string? PaymentStatus { get; set; } // e.g., "Paid", "Unpaid"

    }
}