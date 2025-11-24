namespace KioskAPI.Dtos
{
  using System;
  using System.ComponentModel.DataAnnotations;
  using KioskAPI.Models;

  public class OrderItemDto
  {
    public int ProductId { get; set; }
    public int OrderItemId { get; set; }

    public string Name { get; set; } = string.Empty; // expose product name only
    public Product? Product { get; set; }
    public int Quantity { get; set; }
    public decimal PriceAtPurchase { get; set; }
    public decimal Total => this.Quantity * this.PriceAtPurchase;
    public DateTime AddedAt { get; set; }
    public DateTime ExpiresAt { get; set; }

    // New: Time remaining (calculated)
    public double SecondsRemaining =>
        (this.ExpiresAt - DateTime.UtcNow).TotalSeconds;
  }

  public class CreateOrderItemDto
  {
    [Required]
    public int ProductId { get; set; }
    public int OrderId { get; set; }
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; }
  }
}