namespace KioskAPI.Dtos
{
  using KioskAPI.Models;

  public class AccountDto
  {
    public int UserId { get; set; }
    public User? User { get; set; }
    public decimal Balance { get; set; } = 0m;
    public string? TransactionsSummary { get; set; }
  }
}