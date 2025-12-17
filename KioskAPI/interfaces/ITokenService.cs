namespace KioskAPI.interfaces
{
  using System.Threading.Tasks;
  using KioskAPI.Models;

  public interface ITokenService
  {
    Task<string> GenerateJwtToken(User user);
  }
}