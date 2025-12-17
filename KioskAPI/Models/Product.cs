namespace KioskAPI.Models
{
  using System.ComponentModel.DataAnnotations;
  using System.ComponentModel.DataAnnotations.Schema;

  public class Product
  {
    [Key]
    public int ProductId { get; set; }

    [Required]
    [MaxLength(100, ErrorMessage = "Name length cannot exceed 100 characters.")]
    public string Name { get; set; } = null!;

    [Required]
    [MaxLength(500, ErrorMessage = "Description length cannot exceed 500 characters.")]
    public string? Description { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Required]
    public decimal Price { get; set; } // current price

    [Required]
    [MaxLength(100, ErrorMessage = "category length cannot exceed 100 characters.")]
    public string? Category { get; set; }
    public string? ImageUrl { get; set; }

    [Required]
    [MaxLength(500, ErrorMessage = "Quantity length cannot exceed 500 characters.")]
    public int Quantity { get; set; }

    [ForeignKey(nameof(Supplier))]
    public int? SupplierId { get; set; }
    public Supplier? Supplier { get; set; }

    public bool IsAvailable { get; set; } = true;
  }
}