namespace KioskAPI.Services
{
  using System;
  using System.Collections.Generic;
  using System.IdentityModel.Tokens.Jwt;
  using System.Security.Claims;
  using System.Text;
  using System.Threading.Tasks;
  using KioskAPI.Models;
  using Microsoft.AspNetCore.Identity;
  using Microsoft.IdentityModel.Tokens;

  /// <summary>
  /// Service responsible for generating JSON Web Tokens (JWT) for authenticated users.
  /// </summary>
  public class JwtService
  {
    private readonly IConfiguration _config;
    private readonly UserManager<User> _userManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="JwtService"/> class.
    /// </summary>
    /// <param name="config">Application configuration containing JWT settings.</param>
    /// <param name="userManager">The UserManager instance for retrieving user roles.</param>
    public JwtService(IConfiguration config, UserManager<User> userManager)
    {
      this._config = config;
      this._userManager = userManager;
    }

    /// <summary>
    /// Generates a JWT token for a specified user including their roles.
    /// </summary>
    /// <param name="user">The user for whom the token is generated.</param>
    /// <returns>A JWT as a <see cref="string"/> representing the user's identity and roles.</returns>
    public async Task<string> GenerateTokenAsync(User user)
    {
      var jwtSettings = this._config.GetSection("Jwt");
      var key = jwtSettings["Key"];
      var issuer = jwtSettings["Issuer"];
      var audience = jwtSettings["Audience"];
      var expiryDays = int.Parse(jwtSettings["ExpiryDays"] ?? "7");

      var claims = new List<Claim>
    {
      new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
      new Claim(ClaimTypes.Name, user.Name ?? string.Empty),
      new Claim(ClaimTypes.Email, user.Email ?? string.Empty)
    };

      var roles = await this._userManager.GetRolesAsync(user).ConfigureAwait(true);
      foreach (var role in roles)
      {
        claims.Add(new Claim(ClaimTypes.Role, role));
      }

      var keyBytes = Encoding.UTF8.GetBytes(key);
      var securityKey = new SymmetricSecurityKey(keyBytes);
      var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

      var token = new JwtSecurityToken(
        issuer: issuer,
        audience: audience,
        claims: claims,
        expires: DateTime.UtcNow.AddDays(expiryDays),
        signingCredentials: credentials
      );

      return new JwtSecurityTokenHandler().WriteToken(token);
    }
  }
}