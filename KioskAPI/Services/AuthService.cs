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

      if (await this._context.Users.AnyAsync(u => u.Email == email).ConfigureAwait(true))
      {
        return "User already exists";
      }

      var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

      string roleName = email.EndsWith("@admin.co.za", System.StringComparison.OrdinalIgnoreCase) ? "Admin" : "User";

      var role = await this._context.Roles.FirstOrDefaultAsync(r => r.RoleName == roleName).ConfigureAwait(true);
      if (role == null)
      {
        role = new Role { RoleName = roleName, Description = roleName == "Admin" ? "Administrator" : "Standard user" };
        this._context.Roles.Add(role);
        await this._context.SaveChangesAsync().ConfigureAwait(true);
      }

      var user = new User
      {
        Name = name,
        Email = email,
        PasswordHash = passwordHash,
        RoleId = role.RoleId,
        CreatedAt = System.DateTime.UtcNow,
        UpdatedAt = System.DateTime.UtcNow
      };

      this._context.Users.Add(user);

      var account = new Account
      {
        User = user,
        Balance = 0m,
        LastUpdated = System.DateTime.UtcNow
      };

      this._context.Accounts.Add(account);

      await this._context.SaveChangesAsync().ConfigureAwait(true);

      return $"Registered successfully as {roleName}.";
    }

    public async Task<User?> LoginAsync(string email, string password)
    {
      var user = await this._context.Users
          .Include(u => u.Role)
          .FirstOrDefaultAsync(u => u.Email == email).ConfigureAwait(true);

      if (user == null)
      {
        return null;
      }

      bool isValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
      if (!isValid)
      {
        return null;
      }

      return user;
    }
  }
}
