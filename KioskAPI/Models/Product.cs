using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace KioskAPI.Models
{
    public class Product
    {
         [Key]
        public int ProductId { get; set; }

        [Required]
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; } // current price

        public string? Category { get; set; }
        public string? ImageUrl { get; set; }
        public int Quantity { get; set; }

        [ForeignKey(nameof(Supplier))]
        public int? SupplierId { get; set; }
        public Supplier? Supplier { get; set; }
    }
}