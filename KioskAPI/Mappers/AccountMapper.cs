namespace KioskAPI.Mappers
{
  using KioskAPI.Dtos;
  using KioskAPI.Models;

  /// <summary>
  /// Provides mapping methods between <see cref="Account"/> entities and <see cref="AccountDto"/> data transfer objects.
  /// </summary>
  public class AccountMapper
  {
    /// <summary>
    /// Converts an <see cref="Account"/> entity to an <see cref="AccountDto"/>.
    /// </summary>
    /// <param name="account">The account entity to map.</param>
    /// <returns>An <see cref="AccountDto"/> representing the provided account.</returns>
    public static AccountDto ToDto(Account account)
    {
      return new AccountDto
      {
        User = account.User,
        Balance = account.Balance,
        TransactionsSummary = account.TransactionsSummary
      };
    }
  }
}