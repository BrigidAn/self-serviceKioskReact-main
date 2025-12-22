namespace KioskAPI.Services
{
  using Microsoft.Extensions.Configuration;
  using Microsoft.IdentityModel.Tokens;
  using System;
  using System.Collections.Generic;
  using System.IdentityModel.Tokens.Jwt;
  using System.Security.Claims;
  using System.Text;
  using KioskAPI.Models;
  using Microsoft.AspNetCore.Identity;
  using KioskAPI.interfaces;

  /// <summary>
  /// Service responsible for generating JSON Web Tokens (JWT) for authenticated users.
  /// Implements <see cref="ITokenService"/>.
  /// </summary>
  public class TokenService : ITokenService
  {
    private readonly IConfiguration _config;
    private readonly UserManager<User> _userManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="TokenService"/> class.
    /// </summary>
    /// <param name="config">Application configuration containing JWT settings.</param>
    /// <param name="userManager">UserManager for retrieving user roles.</param>
    public TokenService(IConfiguration config, UserManager<User> userManager)
    {
      this._config = config;
      this._userManager = userManager;
    }

    /// <summary>
    /// Generates a JWT token for the specified <paramref name="user"/>.
    /// Includes user roles as claims.
    /// </summary>
    /// <param name="user">The user for whom the token is generated.</param>
    /// <returns>A JWT as a <see cref="string"/> representing the user's identity and roles.</returns>
    public async Task<string> GenerateJwtToken(User user)
    {
      var roles = await this._userManager.GetRolesAsync(user).ConfigureAwait(true);

      //Retrieve the roles assigned to the user
      var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Name, user.Name ?? "")
            };
      // Add role claims
      foreach (var role in roles)
      {
        claims.Add(new Claim(ClaimTypes.Role, role));
      }

      // Generate signing credentials using the secret key from configuration
      var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this._config["Jwt:Key"]));
      var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

      //Create the JWT token
      var token = new JwtSecurityToken(
          issuer: this._config["Jwt:Issuer"],
          audience: this._config["Jwt:Audience"],
          claims: claims,
          expires: DateTime.UtcNow.AddHours(24),
          signingCredentials: creds
      );

      //Return the serialized token
      return new JwtSecurityTokenHandler().WriteToken(token);
    }
  }
}
