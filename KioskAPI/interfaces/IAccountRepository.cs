namespace KioskAPI.interfaces
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using KioskAPI.Models;

  /// <summary>
  ///  Repository interface for managing user accounts and transactions.
  /// Defines methods for retrieving accounts, transactions, and updating balances.
  /// </summary>
  public interface IAccountRepository
  {
    Task<Account?> GetAccountByUserIdAsync(int userId);
    Task<IEnumerable<Transaction>> GetTransactionsAsync(int userId);
    Task<Account> CreateAccountForUserAsync(int userId);
    Task UpdateBalanceAsync(int userId, decimal amount);
  }
}