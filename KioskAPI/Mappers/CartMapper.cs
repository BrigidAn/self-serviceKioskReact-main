using KioskAPI.Dtos;
using KioskAPI.Models;

public static class CartMapper
{
  /// <summary>
  /// Provides mapping methods between <see cref="Cart"/> entities and <see cref="CartDto"/> data transfer objects.
  /// </summary>
  public static CartDto ToDto(Cart cart)
  {
    /// <summary>
    /// Converts a <see cref="Cart"/> entity to a <see cref="CartDto"/>.
    /// </summary>
    /// <param name="cart">The cart entity to map.</param>
    /// <returns>A <see cref="CartDto"/> representing the provided cart, including all items and total amount.</returns>
    return new CartDto
    {
      CartId = cart.CartId,
      UserId = cart.UserId,
      Items = cart.CartItems.Select(i => new CartItemDto
      {
        CartItemId = i.CartItemId,
        ProductId = i.ProductId,
        ProductName = i.Product.Name,
        Quantity = i.Quantity,
        ImageUrl = i.Product?.ImageUrl ?? null,
        UnitPrice = i.UnitPrice
      }).ToList(),
      TotalAmount = cart.CartItems.Sum(i => i.UnitPrice * i.Quantity)
    };
  }
}
