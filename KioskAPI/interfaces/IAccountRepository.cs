using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KioskAPI.Models;

namespace KioskAPI.interfaces
{
    public interface IAccountRepository
    {
    Task<Account?> GetAccountByUserIdAsync(int userId);
    Task<IEnumerable<Transaction>> GetTransactionsAsync(int userId);
    Task UpdateBalanceAsync(int userId, decimal amount);
    }
}