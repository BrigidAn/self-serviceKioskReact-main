namespace KioskAPI.Models
{
  using System;
  using System.ComponentModel.DataAnnotations;
  using System.ComponentModel.DataAnnotations.Schema;

  public class Transaction
  {
    [Key]
    public int TransactionId { get; set; }

    [ForeignKey(nameof(Account))]
    public int AccountId { get; set; }
    public Account? Account { get; set; }

    [ForeignKey(nameof(Order))]
    public int? OrderId { get; set; }
    public string Type { get; set; } = null!;

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  }
}