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
        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        //check if the user or email is already in the database
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            // Check if email already exists
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest(new { message = "Email already registered." });

            // Hash password with bcrypt
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                PasswordHash = passwordHash,
                RoleId = dto.Email.ToLower() == "don@admin.co.za" ? 1 : 2, // Assign Admin role if email matches
                CreatedAt = DateTime.UtcNow
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Create account for the user with zero balance
            // var account = new Account
            // {
            //     UserId = user.UserId,
            //     Balance = 0m,
            //     LastUpdated = DateTime.UtcNow
            // };

            return Ok(new { message = "Registration successful", role = user.RoleId == 2 ? "User" : "Admin" });
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            // Verify bcrypt password
            var user = await _context.Users.Include(u => u.Role).Include(u => u.Account).FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized(new { message = "Invalid credentials" });

            

            var response = new AuthResponseDto
            {
                Role = user.Role?.RoleName ?? "User",
                Email = user.Email,
                Name = user.Name
            };

            return Ok(response);
        }
    }
}