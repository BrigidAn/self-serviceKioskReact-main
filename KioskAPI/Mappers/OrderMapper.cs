namespace KioskAPI.Mappers
{
  using System.Linq;
  using KioskAPI.Dtos;
  using KioskAPI.Models;

  public class OrderMapper
  {
    public static OrderDto ToDto(Order order)
    {
      return new OrderDto
      {
        OrderId = order.OrderId,
        OrderDate = order.OrderDate,
        TotalAmount = order.TotalAmount,
        Status = order.Status,
        DeliveryMethod = order.DeliveryMethod,
        PaymentStatus = order.PaymentStatus,
        Items = order.OrderItems.Select(i => OrderItemMapper.ToDto(i)).ToList()
      };
    }

    public static Order ToEntity(CreateOrderDto dto)
    {
      return new Order
      {
        UserId = dto.UserId,
        DeliveryMethod = dto.DeliveryMethod
        // other fields filled by service logic
      };
    }
  }
}