using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KioskAPI.Models;

namespace KioskAPI.Dtos
{
    public class TransactionDto
    {
        public int AccountId { get; set; }
        public int? OrderId { get; set; }        
        public string Type { get; set; } = null!;
        public decimal TotalAmount { get; set; }        
        public string? Description { get; set; }
    }
}