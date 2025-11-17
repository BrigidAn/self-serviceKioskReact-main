using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KioskAPI.Dtos
{
    public class ProductFilterDto
    {
    public string? Search { get; set; }        // name or description
    public string? Category { get; set; }      
    public int? SupplierId { get; set; }
    public bool? IsAvailable { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? SortBy { get; set; }        // "price", "name"
    public bool Desc { get; set; } 
     
    }
}