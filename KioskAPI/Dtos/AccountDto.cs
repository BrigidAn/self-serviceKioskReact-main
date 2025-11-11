using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KioskAPI.Models;

namespace KioskAPI.Dtos
{
    public class AccountDto
    {
        public int AccountId { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }

        public decimal Balance { get; set; } = 0m;
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        public string? TransactionsSummary { get; set; }

        public ICollection<Transaction>? Transactions { get; set; }
    }
}