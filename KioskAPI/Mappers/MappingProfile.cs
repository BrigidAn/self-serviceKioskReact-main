namespace KioskAPI.Mappers
{
  using KioskAPI.Dtos;
  using KioskAPI.Models;
  using AutoMapper;

  public class MappingProfile : Profile
  {
    // public MappingProfile()
    // {
    //     CreateMap<Account, AccountDto>();
    //     CreateMap<Transaction, TransactionDto>();

    //     CreateMap<Order, OrderDto>()
    //     .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.User.Name))
    //     .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.OrderItems));

    // CreateMap<OrderItem, OrderItemDto>()
    // .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Product.Name));

    // // Map CreateOrderDto to Order entity for creation
    // CreateMap<CreateOrderDto, Order>();
    // CreateMap<CreateOrderItemDto, OrderItem>();

    // CreateMap<OrderItem, OrderItemDto>()
    //     .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Product.Name));

    // CreateMap<CreateOrderItemDto, OrderItem>()
    //     .ForMember(dest => dest.PriceAtPurchase, opt => opt.Ignore())
    //     .ForMember(dest => dest.OrderItemId, opt => opt.Ignore());

    //     CreateMap<Product, ProductDto>()
    //     .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Name : "Unknown"))
    //     .ForMember(dest => dest.IsAvailable, opt => opt.MapFrom(src => src.Quantity > 0));

    // CreateMap<CreateProductDto, Product>();
    // CreateMap<UpdateProductDto, Product>()
    //     .ForAllMembers(opt => opt.Condition((src, dest, value) => value != null)); // Ignore nulls

    // }

    public MappingProfile()
    {
      // Product mappings
      this.CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Name : "Unknown"))
                .ForMember(dest => dest.IsAvailable, opt => opt.MapFrom(src => src.Quantity > 0));

      this.CreateMap<CreateProductDto, Product>()
                .ForMember(dest => dest.ProductId, opt => opt.Ignore()) // ignore PK
                .ForMember(dest => dest.Supplier, opt => opt.Ignore()); // ignore navigation

      this.CreateMap<UpdateProductDto, Product>()
                .ForMember(dest => dest.ProductId, opt => opt.Ignore())
                .ForMember(dest => dest.Supplier, opt => opt.Ignore())
                .ForAllMembers(opt => opt.Condition((src, dest, value) => value != null)); // only map non-null

      // Order mappings
      this.CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.OrderItems));

      this.CreateMap<CreateOrderDto, Order>()
                .ForMember(dest => dest.OrderId, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.OrderDate, opt => opt.Ignore())
                .ForMember(dest => dest.TotalAmount, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.DeliveryMethod, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentStatus, opt => opt.Ignore())
                .ForMember(dest => dest.OrderItems, opt => opt.Ignore());

      this.CreateMap<OrderItem, OrderItemDto>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : "Unknown"));

      this.CreateMap<CreateOrderItemDto, OrderItem>()
                .ForMember(dest => dest.Order, opt => opt.Ignore())
                .ForMember(dest => dest.Product, opt => opt.Ignore())
                .ForMember(dest => dest.OrderItemId, opt => opt.Ignore())
                .ForMember(dest => dest.PriceAtPurchase, opt => opt.Ignore());

      this.CreateMap<Account, AccountDto>();
      this.CreateMap<Transaction, TransactionDto>();
    }
  }
}
