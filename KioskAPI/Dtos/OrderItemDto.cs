using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KioskAPI.Models;

namespace KioskAPI.Dtos
{
    public class OrderItemDto
    {
        public int ProductId { get; set; }
        public string? Name { get; set; } // expose product name only
        public Product? Product { get; set; }
        public int Quantity { get; set; }
        public decimal PriceAtPurchase { get; set; }
        public decimal Total => Quantity * PriceAtPurchase;
    }

    public class CreateOrderItemDto
    {
        public int ProductId { get; set; }
        public int OrderId { get; set; }
        public int Quantity { get; set; }
    }
}