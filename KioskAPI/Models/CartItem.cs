using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KioskAPI.Models
{
    public class CartItem
    {
    public int CartItemId { get; set; }
    public int UserId { get; set; }

    public int ProductId { get; set; }
    public required Product Product { get; set; } // Navigation property

    public int Quantity { get; set; }

    public DateTime ReservedUntil { get; set; }
    public bool IsCheckedOut { get; set; } = false;
    }
}