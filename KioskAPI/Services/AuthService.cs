namespace KioskAPI.Services
{
  using System.Threading.Tasks;
  using KioskAPI.Data;
  using KioskAPI.Models;
  using Microsoft.EntityFrameworkCore;

  public class AuthService
  {
    private readonly AppDbContext _context;

    public AuthService(AppDbContext context)
    {
      this._context = context;
    }

    public async Task<string> RegisterAsync(string name, string email, string password)
    {
      // Check if user already exists
      if (await this._context.Users.AnyAsync(u => u.Email == email).ConfigureAwait(true))
      {
        return "User already exists";
      }

      // Hash password
      var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

      // Create user (no RoleId anymore)
      var user = new User
      {
        Name = name,
        Email = email,
        PasswordHash = passwordHash,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
      };

      this._context.Users.Add(user);

      // Create zero-balance account
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

    public async Task<User?> LoginAsync(string email, string password)
    {
      // No more role include
      var user = await this._context.Users
          .FirstOrDefaultAsync(u => u.Email == email).ConfigureAwait(true);

      if (user == null)
      {
        return null;
      }

      bool valid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

      return valid ? user : null;
    }
  }
}
