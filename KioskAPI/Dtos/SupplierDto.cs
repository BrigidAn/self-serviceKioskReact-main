namespace KioskAPI.Dtos
{
  public class SupplierDto
  {
    public int SupplierId { get; set; }
    public string Name { get; set; } = null!;
    public string? ContactInfo { get; set; }
  }

  // DTO for creating a new supplier
  public class SupplierCreateDto
  {
    public string Name { get; set; } = null!;
    public string? ContactInfo { get; set; }
  }

  // DTO for updating a supplier
  public class SupplierUpdateDto
  {
    public string? Name { get; set; }
    public string? ContactInfo { get; set; }
  }
}