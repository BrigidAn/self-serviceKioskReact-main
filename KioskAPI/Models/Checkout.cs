namespace KioskAPI.Models
{
  using System;

  using Microsoft.AspNetCore.Identity;

  public class Checkout
  {
    public int CheckoutId { get; set; }

    public int CartId { get; set; }
    public Cart Cart { get; set; }

    public int UserId { get; set; }
    public IdentityUser User { get; set; }

    public decimal TotalAmount { get; set; }

    public string DeliveryMethod { get; set; }
    public DateTime CheckoutDate { get; set; } = DateTime.UtcNow;

    public bool IsConfirmed { get; set; } = false;
    public bool PaymentValidated { get; set; } = false;
  }
}