namespace KioskAPI.Dtos
{
  using System.ComponentModel.DataAnnotations;

  public class SupplierDto
  {

    public int SupplierId { get; set; }
    [Required]
    public string Name { get; set; } = null!;
    [Required]
    public string? ContactInfo { get; set; }
  }

  public class SupplierCreateDto
  {
    [Required]
    public string Name { get; set; } = null!;
    [Required]
    public string? ContactInfo { get; set; }
  }

  public class SupplierUpdateDto
  {

    [Required]
    public string? Name { get; set; }
    [Required]
    public string? ContactInfo { get; set; }
  }
}