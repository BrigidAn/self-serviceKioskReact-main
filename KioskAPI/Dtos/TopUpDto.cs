namespace KioskAPI.Dtos
{
  public class TopUpDto
  {
    // Identity user ID
    public int Id { get; set; }

    // Amount to top up
    public decimal Amount { get; set; }
  }
}