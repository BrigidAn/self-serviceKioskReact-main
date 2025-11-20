namespace KioskAPI.Models
{
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations;

  public class Supplier
  {
    [Key]
    public int SupplierId { get; set; }
    public string Name { get; set; } = null!;
    public string? ContactInfo { get; set; }

    public ICollection<Product>? Products { get; set; }
  }
}