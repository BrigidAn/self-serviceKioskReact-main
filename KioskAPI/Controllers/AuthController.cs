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
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User dto)
        {
            var result = await _authService.RegisterAsync(dto.Name, dto.Email, dto.PasswordHash);
            return Ok(new { message = result });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] User dto)
        {
            var user = await _authService.LoginAsync(dto.Email, dto.PasswordHash);
            if (user == null)
                return Unauthorized(new { message = "Invalid credentials" });

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