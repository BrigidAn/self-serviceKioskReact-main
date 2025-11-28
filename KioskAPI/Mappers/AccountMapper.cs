namespace KioskAPI.Mappers
{
  using KioskAPI.Dtos;
  using KioskAPI.Models;

  public class AccountMapper
  {
    public static AccountDto ToDto(Account account)
    {
      return new AccountDto
      {
        Balance = account.Balance,
        TransactionsSummary = account.TransactionsSummary
      };
    }
  }
}