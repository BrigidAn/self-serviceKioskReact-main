using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KioskAPI.Models;

namespace KioskAPI.Dtos
{
    public class TransactionDto
    {
        public int TransactionId { get; set; }
        public int AccountId { get; set; }
        public Account? Account { get; set; }
        public int? OrderId { get; set; } // optional link to order

        // "credit" or "debit"
        public string Type { get; set; } = null!;
        public decimal TotalAmount { get; set; }        
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}