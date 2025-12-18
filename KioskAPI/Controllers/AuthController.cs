namespace KioskAPI.Controllers
{
  using Microsoft.AspNetCore.Mvc;
  using KioskAPI.Dtos;
  using Microsoft.AspNetCore.Identity;
  using KioskAPI.Models;
  using Microsoft.AspNetCore.Authorization;
  using KioskAPI.Services;

  [ApiController]
  [Route("api/[controller]")]
  public class AuthController : ControllerBase
  {
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly TokenService _tokenService;

    public AuthController(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        TokenService tokenService)
    {
      this._userManager = userManager;
      this._signInManager = signInManager;
      this._tokenService = tokenService;
    }

    // REGISTER
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
      var user = new User
      {
        UserName = dto.Email,
        Email = dto.Email,
        Name = dto.Name
      };

      var result = await this._userManager.CreateAsync(user, dto.Password).ConfigureAwait(true);
      if (!result.Succeeded)
      {
        return this.BadRequest(result.Errors);
      }

      // Default user role
      await this._userManager.AddToRoleAsync(user, "User").ConfigureAwait(true);

      return this.Ok(new { message = "Registered successfully" });
    }

    // LOGIN (JWT)
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
      var user = await this._userManager.FindByEmailAsync(dto.Email).ConfigureAwait(true);
      if (user == null)
      {
        return this.Unauthorized(new { message = "Invalid email or password" });
      }

      var check = await this._signInManager.CheckPasswordSignInAsync(user, dto.Password, false).ConfigureAwait(true);
      if (!check.Succeeded)
      {
        return this.Unauthorized(new { message = "Invalid email or password" });
      }

      // Generate JWT token
      var token = await this._tokenService.GenerateJwtToken(user).ConfigureAwait(true);

      return this.Ok(new
      {
        message = "Login successful",
        token,
        user = new
        {
          user.Id,
          user.Email,
          user.Name,
          roles = await this._userManager.GetRolesAsync(user).ConfigureAwait(true)
        }
      });
    }

    // LOGOUT (JWT)
    [HttpPost("logout")]
    public IActionResult Logout()
    {
      return this.Ok(new { message = "JWT logout = delete token on client." });
    }

    // ASSIGN ROLE (Admin only)
    [HttpPost("assign-role")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AssignRole([FromBody] AssignRoleDto dto)
    {
      var user = await this._userManager.FindByIdAsync(dto.UserId).ConfigureAwait(true);
      if (user == null)
      {
        return this.NotFound(new { message = "User not found" });
      }

      var existingRoles = await this._userManager.GetRolesAsync(user).ConfigureAwait(true);
      await this._userManager.RemoveFromRolesAsync(user, existingRoles).ConfigureAwait(true);
      await this._userManager.AddToRoleAsync(user, dto.Role).ConfigureAwait(true);

      return this.Ok(new { message = $"Role '{dto.Role}' assigned to {user.Email}" });
    }
  }
}
