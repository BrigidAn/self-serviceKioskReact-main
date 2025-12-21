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

  public class JwtService
  {
    private readonly IConfiguration _config;
    private readonly UserManager<User> _userManager;

    public JwtService(IConfiguration config, UserManager<User> userManager)
    {
      this._config = config;
      this._userManager = userManager;
    }

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