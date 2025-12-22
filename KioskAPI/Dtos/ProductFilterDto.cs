namespace KioskAPI.Dtos
{
  public class ProductFilterDto
  {
    public string? Search { get; set; }
    public string? Category { get; set; }
    public bool? IsAvailable { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? SortBy { get; set; }
    public bool Desc { get; set; }
  }
}