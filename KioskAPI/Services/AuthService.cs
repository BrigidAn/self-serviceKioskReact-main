namespace KioskAPI.Services
{
  using System.Threading.Tasks;
  using KioskAPI.Data;
  using KioskAPI.Models;
  using Microsoft.EntityFrameworkCore;

  /// <summary>
  /// Provides authentication-related services such as user registration and login.
  /// </summary>
  public class AuthService
  {
    private readonly AppDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthService"/> class.
    /// </summary>
    /// <param name="context">The database context used for authentication operations.</param>
    public AuthService(AppDbContext context)
    {
      this._context = context;
    }

    public async Task<string> RegisterAsync(string name, string email)
    {
      if (await this._context.Users.AnyAsync(u => u.Email == email).ConfigureAwait(true))
      {
        return "User already exists";
      }
      var user = new User
      {
        Name = name,
        Email = email,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
      };

      this._context.Users.Add(user);
      var account = new Account
      {
        User = user,
        Balance = 0m,
        LastUpdated = DateTime.UtcNow
      };

      this._context.Accounts.Add(account);

      await this._context.SaveChangesAsync().ConfigureAwait(true);

      return "Registered successfully.";
    }

    public async Task<User?> LoginAsync(string email)
    {
      var user = await this._context.Users
          .FirstOrDefaultAsync(u => u.Email == email).ConfigureAwait(true);

      if (user == null)
      {
        return null;
      }

      return user;
    }
  }
}
