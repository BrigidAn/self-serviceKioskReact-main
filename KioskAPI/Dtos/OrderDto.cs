namespace KioskAPI.Dtos
{
  using System;
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations;
  using KioskAPI.Models;

  public class OrderDto
  {
    public int OrderId { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string? DeliveryMethod { get; set; }
    public string? Status { get; set; } // e.g., "Pending", "Completed"
    public string? PaymentStatus { get; set; } // e.g., "Paid", "Unpaid"
    // public User? User { get; set; }
    public List<OrderItemDto>? Items { get; set; }
  }

  public class CreateOrderDto
  {
    public int UserId { get; set; }
    [Required]
    [MinLength(1, ErrorMessage = "Order must contain at least one item.")]
    public List<CreateOrderItemDto> Items { get; set; } = new();
    public string? DeliveryMethod { get; set; }
  }
}