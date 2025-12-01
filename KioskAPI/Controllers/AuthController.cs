namespace KioskAPI.Controllers
{
  using Microsoft.AspNetCore.Mvc;
  using KioskAPI.Dtos;
  using System.Threading.Tasks;
  using Microsoft.AspNetCore.Identity;
  using KioskAPI.Models;
  using KioskAPI.Services;

  [ApiController]
  [Route("api/[controller]")]
  public class AuthController : ControllerBase
  {
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly JwtService _jwtService;

    public AuthController(UserManager<User> userManager, SignInManager<User> signInManager,
          RoleManager<Role> roleManager, JwtService jwtService)
    {
      this._userManager = userManager;
      this._signInManager = signInManager;
      this._roleManager = roleManager;
      this._jwtService = jwtService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
      if (!this.ModelState.IsValid)
      {
        return this.BadRequest(this.ModelState);
      }

      var existingUser = await this._userManager.FindByEmailAsync(dto.Email).ConfigureAwait(true);
      if (existingUser != null)
      {
        return this.BadRequest(new { message = "A user with this email already exists." });
      }

      var newUser = new User
      {
        Name = dto.Name,
        Email = dto.Email,
        UserName = dto.Email,
      };

      var createResult = await this._userManager.CreateAsync(newUser, dto.Password).ConfigureAwait(true);
      if (!createResult.Succeeded)
      {
        return this.BadRequest(new { message = "User registration failed.", errors = createResult.Errors });
      }

      // Assign default role: User
      if (!await this._roleManager.RoleExistsAsync("User").ConfigureAwait(true))
      {
        await this._roleManager.CreateAsync(new Role { Name = "User" }).ConfigureAwait(true);
      }

      await this._userManager.AddToRoleAsync(newUser, "User").ConfigureAwait(true);

      // Generate JWT token
      var token = await this._jwtService.GenerateTokenAsync(newUser).ConfigureAwait(true);
      var roles = await this._userManager.GetRolesAsync(newUser).ConfigureAwait(true);

      return this.Ok(new
      {
        message = "Registration successful",
        token,
        user = new
        {
          newUser.Id,
          newUser.Name,
          newUser.Email,
          Roles = roles
        }
      });
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

      // Generate JWT
      var token = await this._jwtService.GenerateTokenAsync(user).ConfigureAwait(true);
      var roles = await this._userManager.GetRolesAsync(user).ConfigureAwait(true);

      return this.Ok(new
      {
        message = "Login successful",
        token,
        user = new
        {
          user.Id,
          user.Name,
          user.Email,
          Roles = roles
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