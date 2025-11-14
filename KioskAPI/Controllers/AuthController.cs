using Microsoft.AspNetCore.Mvc;
using KioskAPI.Dtos;
using KioskAPI.Services;
using System.Threading.Tasks;

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
            if (string.IsNullOrWhiteSpace(dto.Name) ||
                string.IsNullOrWhiteSpace(dto.Email) ||
                string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest(new { message = "All fields are required." });
            }

            var result = await _authService.RegisterAsync(dto.Name, dto.Email, dto.Password);
            return Ok(new { message = result });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest(new { message = "Email and password are required." });

            var user = await _authService.LoginAsync(dto.Email, dto.Password);
            if (user == null)
                return Unauthorized(new { message = "Invalid credentials" });

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
    }
}
