using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KioskAPI.Models;

namespace KioskAPI.Dtos
{
    public class AccountDto
    {
        public int UserId { get; set; }
        public User? User { get; set; }
        public decimal Balance { get; set; } = 0m;
        public string? TransactionsSummary { get; set; }
    }
}