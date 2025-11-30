namespace KioskAPI.Mappers
{
  using KioskAPI.Dtos;
  using KioskAPI.Models;

  public class OrderItemMapper
  {
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