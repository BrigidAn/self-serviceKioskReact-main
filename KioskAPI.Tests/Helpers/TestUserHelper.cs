using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

public static class TestUserHelper
{
  public static ControllerContext GetControllerContext(int userId, string role = "User")
  {
    var claims = new List<Claim>
    {
      new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
      new Claim(ClaimTypes.Role, role),
      new Claim(ClaimTypes.Email, "test@test.com")
    };

    var identity = new ClaimsIdentity(claims, "TestAuth");
    var user = new ClaimsPrincipal(identity);

    return new ControllerContext
    {
      HttpContext = new DefaultHttpContext
      {
        User = user
      }
    };
  }
}
