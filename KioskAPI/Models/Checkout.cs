namespace KioskAPI.Models
{
  using System;
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations;
  using System.ComponentModel.DataAnnotations.Schema;
  using System.Linq;
  using System.Threading.Tasks;

  using Microsoft.AspNetCore.Identity;

  public class Checkout
  {
    public int CheckoutId { get; set; }

    public int CartId { get; set; }
    public Cart Cart { get; set; }

    public int UserId { get; set; }  // Identity user ID
    public IdentityUser User { get; set; }

    public decimal TotalAmount { get; set; }

    public string DeliveryMethod { get; set; } // "Collection" | "Delivery"
    public DateTime CheckoutDate { get; set; } = DateTime.UtcNow;

    public bool IsConfirmed { get; set; } = false;
    public bool PaymentValidated { get; set; } = false;
  }
}