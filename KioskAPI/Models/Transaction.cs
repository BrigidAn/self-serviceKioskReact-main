using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace KioskAPI.Models
{
    public class Transaction
    {
         [Key]
        public int TransactionId { get; set; }

        [ForeignKey(nameof(Account))]
        public int AccountId { get; set; }
        public Account? Account { get; set; }

        [ForeignKey(nameof(Order))]
        public int? OrderId { get; set; } // optional link to order

        // "credit" or "debit"
        public string Type { get; set; } = null!;


        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }        
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}