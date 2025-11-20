namespace KioskAPI.Dtos
{
  using System;

  public class TransactionDto
  {
    public int AccountId { get; set; }
    public string Type { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
  }
}