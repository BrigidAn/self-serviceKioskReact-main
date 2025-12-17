namespace KioskAPI.Dtos
{
  using System;
  using System.ComponentModel.DataAnnotations;
  using KioskAPI.Models;

  public class CheckoutDto
  {
    public int CartId { get; set; }
    public Cart Cart { get; set; }

    public User User { get; set; }
    public decimal TotalAmount { get; set; }
    public string DeliveryMethod { get; set; } // "Collection" | "Delivery"
    public DateTime CheckoutDate { get; set; } = DateTime.Now;

    public bool IsConfirmed { get; set; } = false;  // Final confirmation
    public bool PaymentValidated { get; set; } = false;
  }

  public class CheckoutRequestDto
  {
    [Required]
    public string DeliveryMethod { get; set; }  // "Collection" or "Delivery"
    public decimal DeliveryFee { get; set; }
    public int? UserId { get; set; }
  }

  public class CheckoutResponseDto
  {
    public int CheckoutId { get; set; }
    public decimal TotalAmount { get; set; }
    public string DeliveryMethod { get; set; }
    public string Message { get; set; } = string.Empty;
    public decimal DeliveryFee { get; set; }
  }
}