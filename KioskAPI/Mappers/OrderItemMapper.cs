namespace KioskAPI.Mappers
{
  using KioskAPI.Dtos;
  using KioskAPI.Models;

  public class OrderItemMapper
  {
    public static OrderItemDto ToDto(OrderItem item)
    {
      return new OrderItemDto
      {
        ProductId = item.ProductId,
        Name = item.Product != null ? item.Product.Name : "Unknown",
        Quantity = item.Quantity,
        PriceAtPurchase = item.PriceAtPurchase
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