namespace KioskAPI.Models
{
  using System.ComponentModel.DataAnnotations;
  using System.ComponentModel.DataAnnotations.Schema;

  public class OrderItem
  {
    [Key]
    public int OrderItemId { get; set; }

    [ForeignKey(nameof(Order))]
    public int OrderId { get; set; }
    public Order? Order { get; set; }

    [ForeignKey(nameof(Product))]
    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int Quantity { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal PriceAtPurchase { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddHours(1);
  }
}