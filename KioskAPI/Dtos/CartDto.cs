namespace KioskAPI.Dtos
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;

  public class CartDto
  {
    public int CartId { get; set; }
    public int UserId { get; set; } // Identity User ID
    public List<CartItemDto> Items { get; set; } = new();
    public decimal TotalAmount { get; set; }
  }
  public class AddToCartDto
  {
    public int UserId { get; set; }      // int ID
    public int ProductId { get; set; }
    public int Quantity { get; set; } = 1;
  }
  public class CartItemDto
  {
    public int CartItemId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice => this.UnitPrice * this.Quantity;
  }
}