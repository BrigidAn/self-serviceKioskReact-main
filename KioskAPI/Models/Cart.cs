namespace KioskAPI.Models
{
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations;

  public class Cart
  {
    [Key]
    public int CartId { get; set; }

    public int UserId { get; set; }               // Identity User int ID
    public User? User { get; set; }   // Navigation property
    public bool IsCheckedOut { get; set; } = false;
    public ICollection<CartItem> CartItems { get; set; }
  }
}
