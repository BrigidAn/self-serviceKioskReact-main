namespace KioskAPI.interfaces
{
  using System.Threading.Tasks;
  using KioskAPI.Models;

  public interface IAuthService
  {
    Task<string> RegisterAsync(string name, string email, string password);
    Task<User?> LoginAsync(string email, string password);
  }
}