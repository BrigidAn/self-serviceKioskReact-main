namespace KioskAPI.interfaces
{
  using System.Threading.Tasks;
  using KioskAPI.Models;

  /// <summary>
  /// Service interface for generating JSON Web Tokens (JWT) for users.
  /// </summary>
  public interface ITokenService
  {
    Task<string> GenerateJwtToken(User user);
  }
}