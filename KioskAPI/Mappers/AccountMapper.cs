namespace KioskAPI.Mappers
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
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