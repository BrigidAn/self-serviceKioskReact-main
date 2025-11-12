using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace KioskAPI.Services
{
    public class AuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public AuthService(AppDbContext context, ICIConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<string> Register(User user, string password)
        {
            if (await _context.Users.AnyAsync(u => u.Email == user.Email)) //when everything matches a user stored in the database 
                return "User already exists";

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password); //hide password
            _context.Users.Add(user);
            await _context.SaveChangesAsync(); // adding a new user onto the platform 
            return "User has been registered succesfully";
        }

        public async Task<string?> Login(string email, string password)
        {
            var user = await _context.Users.FirstorDefaultAsync(u => u.Email == email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)) //confirm password matches user
                return null;

            return GenerateJwtToken(user);
        }
        
        public string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var tokens = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddHours(3),
            signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}