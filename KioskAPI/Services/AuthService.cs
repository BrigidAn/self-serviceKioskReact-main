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

        // Register user
        public async Task<string> RegisterAsync(string name, string email, string password)
        {
            if (await _context.Users.AnyAsync(u => u.Email == email))
                return "Email already exists";

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "User");

            var user = new User
            {
                Name = name,
                Email = email,
                PasswordHash = passwordHash,
                RoleId = userRole?.RoleId ?? 1
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Create account for new user
            var account = new Account
            {
                UserId = user.UserId,
                Balance = 0
            };
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            return "Registration successful";
        }

        // Login user
        public async Task<User?> LoginAsync(string email, string password)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                return null;

            return user;
        }
    }
}