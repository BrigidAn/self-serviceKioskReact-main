namespace KioskAPI.Mappers
{
  using System.Linq;
  using KioskAPI.Dtos;
  using KioskAPI.Models;

  /// <summary>
  /// Provides mapping methods between <see cref="Order"/> entities and <see cref="OrderDto"/> data transfer objects.
  /// </summary>
  public class OrderMapper
  {
    /// <summary>
    /// Maps an <see cref="Order"/> entity to an <see cref="OrderDto"/>.
    /// </summary>
    /// <param name="order">The order entity to map.</param>
    /// <returns>An <see cref="OrderDto"/> representing the order.</returns>
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

    /// <summary>
    /// Maps a <see cref="CreateOrderDto"/> to an <see cref="Order"/> entity.
    /// </summary>
    /// <param name="dto">The DTO containing order creation details.</param>
    /// <returns>A new <see cref="Order"/> entity.</returns>
    public static Order ToEntity(CreateOrderDto dto)
    {
      return new Order
      {
        UserId = dto.UserId,
        DeliveryMethod = dto.DeliveryMethod
      };
    }
  }
}