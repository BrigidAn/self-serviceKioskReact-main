using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KioskAPI.Dtos;
using KioskAPI.Models;
using AutoMapper;

namespace KioskAPI.Mappers
{
   public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Account, AccountDto>();
            CreateMap<Transaction, TransactionDto>();

            CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.User.Name))
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.OrderItems));

        CreateMap<OrderItem, OrderItemDto>()
        .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Product.Name));

        // Map CreateOrderDto to Order entity for creation
        CreateMap<CreateOrderDto, Order>();
        CreateMap<CreateOrderItemDto, OrderItem>();

        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Product.Name));

        CreateMap<CreateOrderItemDto, OrderItem>()
            .ForMember(dest => dest.PriceAtPurchase, opt => opt.Ignore())
            .ForMember(dest => dest.OrderItemId, opt => opt.Ignore());

            CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Name : "Unknown"))
            .ForMember(dest => dest.IsAvailable, opt => opt.MapFrom(src => src.Quantity > 0));

        CreateMap<CreateProductDto, Product>();
        CreateMap<UpdateProductDto, Product>()
            .ForAllMembers(opt => opt.Condition((src, dest, value) => value != null)); // Ignore nulls

        }
    }
}