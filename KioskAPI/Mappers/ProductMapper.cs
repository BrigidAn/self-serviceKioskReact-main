namespace KioskAPI.Mappers
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using KioskAPI.Dtos;
  using KioskAPI.Models;

  public class ProductMapper
  {
    public static ProductDto ToDto(Product product)
    {
      return new ProductDto
      {
        ProductId = product.ProductId,
        Name = product.Name,
        Description = product.Description,
        Price = product.Price,
        Quantity = product.Quantity,
        SupplierName = product.Supplier != null ? product.Supplier.Name : "Unknown",
        IsAvailable = product.Quantity > 0
      };
    }

    public static Product ToEntity(CreateProductDto dto)
    {
      return new Product
      {
        Name = dto.Name,
        Description = dto.Description,
        Price = dto.Price,
        Quantity = dto.Quantity,
        SupplierId = dto.SupplierId
      };
    }

    public static void UpdateEntity(Product product, UpdateProductDto dto)
    {
      if (dto.Name != null)
      {
        product.Name = dto.Name;
      }

      if (dto.Description != null)
      {
        product.Description = dto.Description;
      }

      if (dto.Price.HasValue)
      {
        product.Price = dto.Price.Value;
      }

      if (dto.Quantity.HasValue)
      {
        product.Quantity = dto.Quantity.Value;
      }

      if (dto.SupplierId.HasValue)
      {
        product.SupplierId = dto.SupplierId.Value;
      }
    }
  }
}