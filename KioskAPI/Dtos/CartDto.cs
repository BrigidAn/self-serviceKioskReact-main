namespace KioskAPI.Dtos
{
  using System.Collections.Generic;

  public class CartDto
  {
    public int CartId { get; set; }
    public int UserId { get; set; } // Identity User ID
    public List<CartItemDto> Items { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public DateTime? ExpiresAt { get; set; }
  }
  public class AddToCartDto
  {
    public int ProductId { get; set; }
    public int Quantity { get; set; } = 1;
    public DateTime? ExpiresAt { get; set; }
  }
  public class CartItemDto
  {
    public int CartItemId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public decimal TotalPrice => this.UnitPrice * this.Quantity;
  }
}