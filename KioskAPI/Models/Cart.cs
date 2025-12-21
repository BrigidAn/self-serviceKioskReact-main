namespace KioskAPI.Models
{
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations;

  public class Cart
  {
    [Key]
    public int CartId { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }
    public bool IsCheckedOut { get; set; } = false;
    public DateTime? ExpiresAt { get; set; }

    public ICollection<CartItem> CartItems { get; set; }
  }
}
