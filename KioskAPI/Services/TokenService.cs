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

  public class TokenService
  {
    private readonly IConfiguration _config;
    private readonly UserManager<User> _userManager;

    public TokenService(IConfiguration config, UserManager<User> userManager)
    {
      this._config = config;
      this._userManager = userManager;
    }

    public async Task<string> GenerateJwtToken(User user)
    {
      var roles = await this._userManager.GetRolesAsync(user).ConfigureAwait(true);

      var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Name, user.Name ?? "")
            };

      // Add roles
      foreach (var role in roles)
      {
        claims.Add(new Claim(ClaimTypes.Role, role));
      }

      var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this._config["Jwt:Key"]));
      var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

      var token = new JwtSecurityToken(
          issuer: this._config["Jwt:Issuer"],
          audience: this._config["Jwt:Audience"],
          claims: claims,
          expires: DateTime.UtcNow.AddHours(24),
          signingCredentials: creds
      );

      return new JwtSecurityTokenHandler().WriteToken(token);
    }
  }
}
