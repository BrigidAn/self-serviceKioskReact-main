using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace KioskAPI.Models
{
    public class Order
    {
         [Key]
        public int OrderId { get; set; }

        [ForeignKey(nameof(User))]
        public int UserId { get; set; }
        public User? User { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public decimal TotalAmount { get; set; }
        public string? Status { get; set; } // e.g., "Pending", "Completed"
        public string? DeliveryMethod { get; set; }
        public string? PaymentStatus { get; set; } // e.g., "Paid", "Unpaid"

        public ICollection<OrderItem>? OrderItems { get; set; }
    }
}