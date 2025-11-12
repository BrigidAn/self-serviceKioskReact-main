using System.Threading.Tasks;
using KioskAPI.Data;
using KioskAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace KioskAPI.Services
{
    public class AuthService
    {
        private readonly AppDbContext _context;

        public AuthService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<string> RegisterAsync(string name, string email, string password)
        {

            if (await _context.Users.AnyAsync(u => u.Email == email))
                return "User already exists";

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            string roleName = email.EndsWith("@admin.co.za", System.StringComparison.OrdinalIgnoreCase) ? "Admin" : "User";

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == roleName);
            if (role == null)
            {
                role = new Role { RoleName = roleName, Description = roleName == "Admin" ? "Administrator" : "Standard user" };
                _context.Roles.Add(role);
                await _context.SaveChangesAsync();
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

            _context.Users.Add(user);

            var account = new Account
            {
                User = user,
                Balance = 0m,
                LastUpdated = System.DateTime.UtcNow
            };

            _context.Accounts.Add(account);

            await _context.SaveChangesAsync();

            return $"Registered successfully as {roleName}.";
        }
        
        public async Task<User?> LoginAsync(string email, string password)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
                return null;


            bool isValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            if (!isValid)
                return null;

            return user;
        }
    }
}
