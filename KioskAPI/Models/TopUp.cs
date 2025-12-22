namespace KioskAPI.Models
{
  using System;
  using System.ComponentModel.DataAnnotations;

  public class TopUp
  {
    [Required]
    public int Id { get; set; }

    [Required]
    [Range(1, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
    public decimal Amount { get; set; }
  }
}