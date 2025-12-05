namespace KioskAPI.Dtos
{
  using System.ComponentModel.DataAnnotations;
  using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

  public class AdminDto
  {
    [Required]
    public string UserName { get; set; } = string.Empty;
    [Required]
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "Admin";
  }

  public class AdminTopUpDo
  {
    [Required(ErrorMessage = "UserId is required")]
    public int UserId { get; set; }

    [Required]
    [Range(0.01, 1500, ErrorMessage = "Amount must be greater than zero")]
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
  }
}