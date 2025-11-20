namespace KioskAPI.Models
{
  using System;
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations;
  using System.ComponentModel.DataAnnotations.Schema;

  public class Order
  {
    [Key]
    public int OrderId { get; set; }

    [ForeignKey(nameof(User))]
    public int UserId { get; set; }
    public User? User { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }
    public string? Status { get; set; } // e.g., "Pending", "Completed"
    public string? DeliveryMethod { get; set; }
    public string? PaymentStatus { get; set; } // e.g., "Paid", "Unpaid"

    public ICollection<OrderItem>? OrderItems { get; set; }
  }
}