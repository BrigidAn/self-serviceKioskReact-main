namespace KioskAPI.Controllers
{
  using Microsoft.AspNetCore.Mvc;
  using KioskAPI.Dtos;
  using Microsoft.AspNetCore.Identity;
  using KioskAPI.Models;
  using Microsoft.AspNetCore.Authorization;
  using KioskAPI.Services;

  /// <summary>
  /// Controller responsible for authentication such as
  /// user registration, login, logout , and role assign
  /// </summary>
  [ApiController]
  [Route("api/[controller]")]
  public class AuthController : ControllerBase
  {
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly TokenService _tokenService;

    /// <summary>
    /// Initializes a new instance of the authcontroller
    /// </summary>
    /// <param name="userManager">Injected UserManager for user management</param>
    /// <param name="signInManager">Injected SignInMAnager fro authentication</param>
    /// <param name="tokenService">Injected TokenService to generate JWT token</param>
    public AuthController(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        TokenService tokenService)
    {
      this._userManager = userManager;
      this._signInManager = signInManager;
      this._tokenService = tokenService;
    }

    /// <summary>
    /// Registers a new user with email, name, and password
    /// </summary>
    /// <param name="dto">register dto containing Email, name, and password</param>
    /// <returns>
    /// 200 OK if registration succeeded,
    /// 400 Bad Request if registration failed due to validation errors.
    /// </returns>
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

      await this._userManager.AddToRoleAsync(user, "User").ConfigureAwait(true);

      return this.Ok(new { message = "Registered successfully" });
    }

    /// <summary>
    /// Authenticates a user with email and password, returning a JWT token if successful.
    /// </summary>
    /// <param name="dto">LoginDto containing Email and Password.</param>
    /// <returns>
    /// 200 OK with token and user info if successful,
    /// 401 Unauthorized if email or password is invalid.
    /// </returns>
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

    /// <summary>
    /// Logs out a user by instructing client to delete JWT token.
    /// </summary>
    /// <returns>200 OK with message about logout.</returns>
    [HttpPost("logout")]
    public IActionResult Logout()
    {
      return this.Ok(new { message = "JWT logout = delete token on client." });
    }

    /// <summary>
    /// Assigns a role to a user. Only accessible by Admins.
    /// </summary>
    /// <param name="dto">AssignRoleDto containing UserId and Role.</param>
    /// <returns>
    /// 200 OK if role assigned successfully,
    /// 400 Bad Request if UserId or Role is missing,
    /// 404 Not Found if user does not exist,
    /// 401 Unauthorized if the requester is not an Admin.
    /// </returns>
    [HttpPost("assign-role")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AssignRole([FromBody] AssignRoleDto dto)
    {
      if (dto == null || string.IsNullOrEmpty(dto.UserId) || string.IsNullOrEmpty(dto.Role))
      {
        return this.BadRequest(new { message = "UserId and Role are required." });
      }

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
