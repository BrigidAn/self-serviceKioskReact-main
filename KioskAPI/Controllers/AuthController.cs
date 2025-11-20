namespace KioskAPI.Controllers
{
  using Microsoft.AspNetCore.Mvc;
  using KioskAPI.Dtos;
  using KioskAPI.Services;
  using System.Threading.Tasks;
  using System.Linq;
  using KioskAPI.interfaces;
  using Microsoft.AspNetCore.Identity;
  using KioskAPI.Models;

  [ApiController]
  [Route("api/[controller]")]
  public class AuthController : ControllerBase
  {
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;

    public AuthController(UserManager<User> userManager, SignInManager<User> signInManager)
    {
      this._userManager = userManager;
      this._signInManager = signInManager;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
      if (string.IsNullOrWhiteSpace(dto.Name) ||
          string.IsNullOrWhiteSpace(dto.Email) ||
          string.IsNullOrWhiteSpace(dto.Password))
      {
        return this.BadRequest(new { message = "All fields are required." });
      }

      if (!this.IsValidEmail(dto.Email))
      {
        return this.BadRequest(new { message = "Invalid email format." });
      }

      if (!this.IsValidPassword(dto.Password))
      {
        return this.BadRequest(new { message = "Password must be at least 8 characters and include uppercase, lowercase, digit, and special character." });
      }

      var existingUser = await this._userManager.FindByEmailAsync(dto.Email).ConfigureAwait(true);
      if (existingUser != null)
      {
        return this.BadRequest(new { message = "Email is already registered." });
      }

      var user = new User { UserName = dto.Email, Email = dto.Email, Name = dto.Name };
      var result = await this._userManager.CreateAsync(user, dto.Password).ConfigureAwait(true);

      if (!result.Succeeded)
      {
        return this.BadRequest(result.Errors);
      }

      // Default role is "User"
      await this._userManager.AddToRoleAsync(user, "User").ConfigureAwait(true);

      return this.Ok(new { message = "Registered successfully", role = "User" });
    }

    // LOGIN
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
      if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
      {
        return this.BadRequest(new { message = "Email and password are required." });
      }

      var user = await this._userManager.FindByEmailAsync(dto.Email).ConfigureAwait(true);
      if (user == null)
      {
        return this.Unauthorized(new { message = "Invalid email or password." });
      }

      var result = await this._signInManager.CheckPasswordSignInAsync(user, dto.Password, false).ConfigureAwait(true);
      if (!result.Succeeded)
      {
        return this.Unauthorized(new { message = "Invalid email or password." });
      }

      // Sign in user
      await this._signInManager.SignInAsync(user, true).ConfigureAwait(true);

      // Store UserId in session
      this.HttpContext.Session.SetString("UserId", user.Id.ToString());

      return this.Ok(new
      {
        message = "Login successful",
        user = new
        {
          user.Id,
          user.Name,
          user.Email,
          Roles = await this._userManager.GetRolesAsync(user).ConfigureAwait(true)
        }
      });
    }

    // LOGOUT
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
      await this._signInManager.SignOutAsync().ConfigureAwait(true);
      this.HttpContext.Session.Remove("UserId");
      return this.Ok(new { message = "Logged out successfully." });
    }

    [HttpPost("assign-role")]
    public async Task<IActionResult> AssignRole([FromQuery] string userId, [FromQuery] string role)
    {
      var adminId = this.HttpContext.Session.GetString("UserId");
      if (adminId == null)
      {
        return this.Unauthorized();
      }

      var currentUser = await this._userManager.FindByIdAsync(adminId).ConfigureAwait(true);
      if (!await this._userManager.IsInRoleAsync(currentUser, "Admin").ConfigureAwait(true))
      {
        return this.Unauthorized(new { message = "Only admins can assign roles." });
      }

      var user = await this._userManager.FindByIdAsync(userId).ConfigureAwait(true);
      if (user == null)
      {
        return this.NotFound(new { message = "User not found." });
      }

      // Remove old roles
      var oldRoles = await this._userManager.GetRolesAsync(user).ConfigureAwait(true);
      await this._userManager.RemoveFromRolesAsync(user, oldRoles).ConfigureAwait(true);

      // Assign new role
      await this._userManager.AddToRoleAsync(user, role).ConfigureAwait(true);

      return this.Ok(new { message = $"Role '{role}' assigned to {user.Name}" });
    }

    // Helper: Validate Email
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

    // Helper: Validate Password
    private bool IsValidPassword(string password)
    {
      if (password.Length < 8)
      {
        return false;
      }

      bool hasUpper = false, hasLower = false, hasDigit = false, hasSpecial = false;

      foreach (var c in password)
      {
        if (char.IsUpper(c))
        {
          hasUpper = true;
        }
        else if (char.IsLower(c))
        {
          hasLower = true;
        }
        else if (char.IsDigit(c))
        {
          hasDigit = true;
        }
        else
        {
          hasSpecial = true;
        }
      }

      return hasUpper && hasLower && hasDigit && hasSpecial;
    }
  }
}