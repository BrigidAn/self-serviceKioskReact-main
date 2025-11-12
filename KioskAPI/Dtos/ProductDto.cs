using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KioskAPI.Models;

namespace KioskAPI.Dtos
{
    public class ProductDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; } // current price
        public string? Category { get; set; }
        public string? ImageUrl { get; set; }
        public int Quantity { get; set; }
        public int? SupplierId { get; set; }
    }
}