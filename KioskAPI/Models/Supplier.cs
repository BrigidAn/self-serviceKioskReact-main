using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace KioskAPI.Models
{
    public class Supplier
    {
           [Key]
        public int SupplierId { get; set; }
        public string Name { get; set; } = null!;
        public string? ContactInfo { get; set; }

        public ICollection<Product>? Products { get; set; }
    }
}