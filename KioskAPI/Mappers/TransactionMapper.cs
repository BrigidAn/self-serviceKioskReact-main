namespace KioskAPI.Mappers
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using KioskAPI.Dtos;
  using KioskAPI.Models;

  public class TransactionMapper
  {
    public static TransactionDto ToDto(Transaction t)
    {
      return new TransactionDto
      {
        Type = t.Type,
        TotalAmount = t.TotalAmount,
        Description = t.Description,
        CreatedAt = t.CreatedAt
      };
    }
  }
}