using KioskAPI.Dtos;
using KioskAPI.Models;

public static class CartMapper
{
  public static CartDto ToDto(Cart cart)
  {
    return new CartDto
    {
      CartId = cart.CartId,
      Items = cart.CartItems.Select(i => new CartItemDto
      {
        CartItemId = i.CartItemId,
        ProductId = i.ProductId,
        ProductName = i.Product.Name,
        Quantity = i.Quantity,
        UnitPrice = i.UnitPrice
      }).ToList(),
      TotalAmount = cart.CartItems.Sum(i => i.UnitPrice * i.Quantity)
    };
  }
}
