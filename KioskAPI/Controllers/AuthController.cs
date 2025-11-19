using Microsoft.AspNetCore.Mvc;
using KioskAPI.Dtos;
using KioskAPI.Services;
using System.Threading.Tasks;
using System.Linq;
using KioskAPI.interfaces;

namespace KioskAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            // 1. Required fields check
            if (string.IsNullOrWhiteSpace(dto.Name) ||
                string.IsNullOrWhiteSpace(dto.Email) ||
                string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest(new { message = "All fields are required." });
            }

            // 2. Email format validation
            if (!IsValidEmail(dto.Email))
                return BadRequest(new { message = "Invalid email format." });

            // 3. Password rule validation
            if (!IsValidPassword(dto.Password))
            {
                return BadRequest(new
                {
                    message = "Password must be at least 8 characters and include uppercase, lowercase, digit, and special character."
                });
            }

            // 4. Register user through AuthService (AuthService should handle duplicate email check)
            var result = await _authService.RegisterAsync(dto.Name, dto.Email, dto.Password);

            if (result == "EmailExists")
                return BadRequest(new { message = "Email is already registered." });

            return Ok(new { message = "Registration successful" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) ||
                string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest(new { message = "Email and password are required." });
            }

            if (!IsValidEmail(dto.Email))
                return BadRequest(new { message = "Invalid email format." });

            var user = await _authService.LoginAsync(dto.Email, dto.Password);

            if (user == null)
                return Unauthorized(new { message = "Invalid email or password." });

            HttpContext.Session.SetInt32("UserId", user.UserId);

            return Ok(new
            {
                message = "Login successful",
                user = new
                {
                    user.UserId,
                    user.Name,
                    user.Email,
                    Role = user.Role?.RoleName
                }
            });
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidPassword(string password)
        {
            if (password.Length < 8)
                return false;

            bool hasUpper = password.Any(char.IsUpper);
            bool hasLower = password.Any(char.IsLower);
            bool hasDigit = password.Any(char.IsDigit);
            bool hasSpecial = password.Any(ch => !char.IsLetterOrDigit(ch));

            return hasUpper && hasLower && hasDigit && hasSpecial;
        }
    }
}
