namespace KioskAPI.Mappers
{
  using KioskAPI.Dtos;
  using KioskAPI.Models;

  /// <summary>
  /// Provides mapping methods between <see cref="OrderItem"/> entities and corresponding DTOs.
  /// </summary>
  public class OrderItemMapper
  {

    /// <summary>
    /// Maps an <see cref="OrderItem"/> entity to an <see cref="OrderItemDto"/>.
    /// Returns null if the input entity is null.
    /// </summary>
    /// <param name="item">The order item entity to map.</param>
    /// <returns>An <see cref="OrderItemDto"/> representing the order item, or null if input is null.</returns>
    public static OrderItemDto? ToDto(OrderItem item)
    {
      if (item == null)
      {
        return null;
      }

      return new OrderItemDto
      {
        ProductId = item.ProductId,
        Name = item.Product != null ? item.Product.Name : "Unknown",
        Quantity = item.Quantity,
        PriceAtPurchase = item.PriceAtPurchase,
        AddedAt = item.AddedAt,
        ExpiresAt = item.ExpiresAt
      };
    }

    /// <summary>
    /// Maps a <see cref="CreateOrderItemDto"/> to an <see cref="OrderItem"/> entity.
    /// </summary>
    /// <param name="dto">The DTO containing order creation details.</param>
    /// <returns>A new <see cref="OrderItem"/> entity.</returns>
    public static OrderItem ToEntity(CreateOrderItemDto dto)
    {
      return new OrderItem
      {
        ProductId = dto.ProductId,
        Quantity = dto.Quantity
      };
    }
  }
}