namespace KioskAPI.Mappers
{
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
        ImageUrl = product.ImageUrl,
        SupplierId = product.SupplierId ?? 0,
        SupplierName = product.Supplier?.Name ?? "Unknown",
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
        SupplierId = dto.SupplierId,
        Category = dto.Category,
        ImageUrl = dto.ImageUrl,
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

      if (dto.SupplierId.HasValue)
      {
        product.SupplierId = dto.SupplierId.Value;
      }

      if (dto.Category != null)
      {
        product.Category = dto.Category;
      }

      if (dto.ImageUrl != null)
      {
        product.ImageUrl = dto.ImageUrl;
      }
    }
  }
}