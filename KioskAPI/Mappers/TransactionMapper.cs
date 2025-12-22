namespace KioskAPI.Mappers
{
  using KioskAPI.Dtos;
  using KioskAPI.Models;

  /// <summary>
  /// Provides mapping methods between <see cref="Transaction"/> entities and <see cref="TransactionDto"/> objects.
  /// </summary>
  public class TransactionMapper
  {
    /// <summary>
    /// Maps a <see cref="Transaction"/> entity to a <see cref="TransactionDto"/>.
    /// </summary>
    /// <param name="t">The transaction entity to map.</param>
    /// <returns>A <see cref="TransactionDto"/> representing the transaction.</returns>
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