namespace KioskAPI.interfaces
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using KioskAPI.Models;

  public interface IAccountRepository
  {
    Task<Account?> GetAccountByUserIdAsync(int userId);
    Task<IEnumerable<Transaction>> GetTransactionsAsync(int userId);
    Task UpdateBalanceAsync(int userId, decimal amount);
  }
}