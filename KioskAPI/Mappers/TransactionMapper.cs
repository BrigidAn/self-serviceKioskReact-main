namespace KioskAPI.Mappers
{
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