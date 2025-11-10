using KioskAPI.Data;
using KioskAPI.Dtos;
using KioskAPI.Models;
using KioskAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KioskAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly JwtService _jwt;

        public AuthController(AppDbContext context, JwtService jwt)
        {
            _context = context;
            _jwt = jwt;
        }

        //check if the user or email is already in the database
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            // Check if email already exists
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest(new { message = "Email already registered." });

            // By default assign the "User" role (role id 2 seeded earlier)
            var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "User");
            if (userRole == null) return StatusCode(500, "Roles not seeded");

            // Hash password with bcrypt
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                PasswordHash = passwordHash,
                RoleId = userRole.RoleId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Create account for the user with zero balance
            var account = new Account
            {
                UserId = user.UserId,
                Balance = 0m,
                LastUpdated = DateTime.UtcNow
            };

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Registration successful" });
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _context.Users.Include(u => u.Role).Include(u => u.Account).FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
                return Unauthorized(new { message = "Invalid credentials" });

            // Verify bcrypt password
            bool verified = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
            if (!verified) return Unauthorized(new { message = "Invalid credentials" });

            // Generate JWT with role claim
            var token = _jwt.GenerateToken(user, user.Role?.RoleName ?? "User");

            var response = new AuthResponseDto
            {
                Token = token,
                Role = user.Role?.RoleName ?? "User",
                Email = user.Email,
                Name = user.Name
            };

            return Ok(response);
        }
    }
}