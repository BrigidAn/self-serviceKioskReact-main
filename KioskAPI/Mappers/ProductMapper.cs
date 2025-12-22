namespace KioskAPI.Mappers
{
  using KioskAPI.Dtos;
  using KioskAPI.Models;

  /// <summary>
  /// Provides mapping methods between <see cref="Product"/> entities and related DTOs.
  /// </summary>
  public class ProductMapper
  {
    /// <summary>
    /// Maps a <see cref="Product"/> entity to a <see cref="ProductDto"/>.
    /// </summary>
    /// <param name="product">The product entity to map.</param>
    /// <returns>A <see cref="ProductDto"/> representing the product.</returns>
    public static ProductDto ToDto(Product product)
    {
      return new ProductDto
      {
        ProductId = product.ProductId,
        Name = product.Name,
        Description = product.Description,
        Price = product.Price,
        Quantity = product.Quantity,
        Category = product.Category,
        ImageUrl = product.ImageUrl,
        SupplierId = product.SupplierId ?? 0,
        SupplierName = product.Supplier?.Name ?? "Unknown",
        IsAvailable = product.Quantity > 0
      };
    }

    /// <summary>
    /// Maps a <see cref="CreateProductDto"/> to a <see cref="Product"/> entity.
    /// </summary>
    /// <param name="dto">The DTO containing product creation data.</param>
    /// <returns>A new <see cref="Product"/> entity.</returns>
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
        IsAvailable = dto.IsAvailable,
      };
    }

    /// <summary>
    /// Updates an existing <see cref="Product"/> entity with values from an <see cref="UpdateProductDto"/>.
    /// Only non-null properties in the DTO will update the entity.
    /// </summary>
    /// <param name="product">The product entity to update.</param>
    /// <param name="dto">The DTO containing updated product data.</param>
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
      if (dto.IsAvailable != null)
      {
        product.IsAvailable = dto.IsAvailable;
      }
    }
  }
}