namespace KioskAPI.Dtos
{
  using System;
  using System.ComponentModel.DataAnnotations;

  public class ProductDto
  {
    public int ProductId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string Category { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }

    public int Quantity { get; set; }
    public int SupplierId { get; set; }

    public string? SupplierName { get; set; }

    public bool IsAvailable { get; set; }
    public bool DisplayAvailable => IsAvailable && Quantity > 0;
  }

  public class CreateProductDto
  {
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;
    [Required]
    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
    public decimal Price { get; set; }

    [Required]
    [MaxLength(50)]
    public string Category { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    [Required(ErrorMessage = "ImageUrl is Required")]
    public IFormFile File { get; set; }

    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Quantity cannot be negative.")]
    public int Quantity { get; set; }
    [Required]
    public int? SupplierId { get; set; }

    public bool IsAvailable { get; set; } = true;

  }

  public class UpdateProductDto
  {
    [Required]
    [MaxLength(100)]
    public string? Name { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price Must be greater than zero")]
    public decimal? Price { get; set; }

    [Required]
    [MaxLength(50)]
    public string? Category { get; set; }
    public string? ImageUrl { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Quantity can not be negative")]
    public int? Quantity { get; set; }
    public int? SupplierId { get; set; }
    public IFormFile File { get; set; }

    public bool IsAvailable { get; set; }
  }
}