using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace KioskAPI.Models
{
    public class Account
    {
        [Key]
        public int AccountId { get; set; }

        [ForeignKey(nameof(User))]
        public int UserId { get; set; }
        public User? User { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Balance { get; set; } = 0m;
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        public string? TransactionsSummary { get; set; }

        public ICollection<Transaction>? Transactions { get; set; }
    }
}