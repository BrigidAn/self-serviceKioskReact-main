namespace KioskAPI.interfaces
{
  using System.Threading.Tasks;
  using KioskAPI.Models;

  /// <summary>
  /// Service interface for handling authentication operations such as
  /// user registration and login.
  /// </summary>
  public interface IAuthService
  {
    Task<string> RegisterAsync(string name, string email, string password);
    Task<User?> LoginAsync(string email, string password);
  }
}