using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using KioskAPI.Models;

namespace KioskAPI.Dtos
{
    public class ProductDto
    {
    public int ProductId { get; set; }

    [Required]
    [MaxLength(100, ErrorMessage = "Name length cannot exceed 100 characters.")]
    public string Name { get; set; } = null!;

    [Required]
    [MaxLength(500, ErrorMessage = "Description length cannot exceed 500 characters.")]
    public string? Description { get; set; }

    [Required]
    public decimal Price { get; set; } // price for product

    [Required]
    [MaxLength(50, ErrorMessage = "Category length cannot exceed 50 characters.")]
    public string? Category { get; set; } 
    public string? ImageUrl { get; set; }

    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Quantity cannot be negative.")]
    public int Quantity { get; set; }
    public string? SupplierName { get; set; }
    public bool IsAvailable { get; set; }

    }

        public class CreateProductDto
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? Category { get; set; }
        public string? ImageUrl { get; set; }
        public int Quantity { get; set; }
        public int? SupplierId { get; set; }
    }

    public class UpdateProductDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public string? Category { get; set; }
        public string? ImageUrl { get; set; }
        public int? Quantity { get; set; }
        public int? SupplierId { get; set; }
    }
}