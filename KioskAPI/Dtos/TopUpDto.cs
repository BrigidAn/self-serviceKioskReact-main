namespace KioskAPI.Dtos
{
  using System.ComponentModel.DataAnnotations;

  public class TopUpDto
  {
    // Identity user ID
    [Required]
    public int Id { get; set; }

    [Range(0.01, 1500, ErrorMessage = "Maximum amount is 1500 per deposit")]
    public decimal Amount { get; set; }
  }
}