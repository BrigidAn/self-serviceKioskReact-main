using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using KioskAPI.Controllers;
using KioskAPI.Dtos;
using KioskAPI.Models;
using KioskAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

// Fake TokenService that inherits from TokenService

public class AuthControllerTests
{
  private static Mock<UserManager<User>> MockUserManager()
  {
    var store = new Mock<IUserStore<User>>();
    return new Mock<UserManager<User>>(
        store.Object, null, null, null, null, null, null, null, null);
  }

  private static Mock<SignInManager<User>> MockSignInManager(UserManager<User> userManager)
  {
    return new Mock<SignInManager<User>>(
        userManager,
        new Mock<IHttpContextAccessor>().Object,
        new Mock<IUserClaimsPrincipalFactory<User>>().Object,
        null, null, null, null);
  }

  private static AuthController CreateController(
      Mock<UserManager<User>> userManager,
      Mock<SignInManager<User>> signInManager)
  {
    // Mock IConfiguration
    var configMock = new Mock<IConfiguration>();

    configMock.Setup(c => c["Jwt:Key"])
        .Returns("THIS_IS_A_VERY_SECRET_TEST_KEY_123456789");

    configMock.Setup(c => c["Jwt:Issuer"])
        .Returns("TestIssuer");

    configMock.Setup(c => c["Jwt:Audience"])
        .Returns("TestAudience");

    var tokenService = new TokenService(configMock.Object, userManager.Object);

    return new AuthController(
        userManager.Object,
        signInManager.Object,
        tokenService);
  }

  // ---------------- REGISTER ----------------
  [Fact]
  public async Task Register_Success_ReturnsOk()
  {
    var userManager = MockUserManager();
    var signInManager = MockSignInManager(userManager.Object);

    userManager.Setup(u => u.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
        .ReturnsAsync(IdentityResult.Success);

    userManager.Setup(u => u.AddToRoleAsync(It.IsAny<User>(), "User"))
        .ReturnsAsync(IdentityResult.Success);

    var controller = CreateController(userManager, signInManager);

    var result = await controller.Register(new RegisterDto
    {
      Email = "test@test.com",
      Password = "Password123!",
      Name = "Test User"
    });

    result.Should().BeOfType<OkObjectResult>();
  }

  [Fact]
  public async Task Register_Failure_ReturnsBadRequest()
  {
    var userManager = MockUserManager();
    var signInManager = MockSignInManager(userManager.Object);

    userManager.Setup(u => u.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
        .ReturnsAsync(IdentityResult.Failed(
            new IdentityError { Description = "Error" }));

    var controller = CreateController(userManager, signInManager);

    var result = await controller.Register(new RegisterDto
    {
      Email = "test@test.com",
      Password = "bad",
      Name = "Test"
    });

    result.Should().BeOfType<BadRequestObjectResult>();
  }

  // ---------------- LOGIN ----------------
  [Fact]
  public async Task Login_UserNotFound_ReturnsUnauthorized()
  {
    var userManager = MockUserManager();
    var signInManager = MockSignInManager(userManager.Object);

    userManager.Setup(u => u.FindByEmailAsync(It.IsAny<string>()))
        .ReturnsAsync((User)null);

    var controller = CreateController(userManager, signInManager);

    var result = await controller.Login(new LoginDto
    {
      Email = "missing@test.com",
      Password = "Password123!"
    });

    result.Should().BeOfType<UnauthorizedObjectResult>();
  }

  [Fact]
  public async Task Login_InvalidPassword_ReturnsUnauthorized()
  {
    var userManager = MockUserManager();
    var signInManager = MockSignInManager(userManager.Object);

    var user = new User { Email = "test@test.com" };

    userManager.Setup(u => u.FindByEmailAsync(user.Email))
        .ReturnsAsync(user);

    signInManager.Setup(s => s.CheckPasswordSignInAsync(user, It.IsAny<string>(), false))
        .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

    var controller = CreateController(userManager, signInManager);

    var result = await controller.Login(new LoginDto
    {
      Email = user.Email,
      Password = "wrong"
    });

    result.Should().BeOfType<UnauthorizedObjectResult>();
  }

  [Fact]
  public async Task Login_Success_ReturnsToken()
  {
    var userManager = MockUserManager();
    var signInManager = MockSignInManager(userManager.Object);

    var user = new User
    {
      Id = 1,
      Email = "test@test.com",
      Name = "Test"
    };

    userManager.Setup(u => u.FindByEmailAsync(user.Email))
        .ReturnsAsync(user);

    signInManager.Setup(s => s.CheckPasswordSignInAsync(user, It.IsAny<string>(), false))
        .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

    userManager.Setup(u => u.GetRolesAsync(user))
        .ReturnsAsync(new List<string> { "User" });

    var controller = CreateController(userManager, signInManager);

    var result = await controller.Login(new LoginDto
    {
      Email = user.Email,
      Password = "Password123!"
    });

    var ok = result as OkObjectResult;
    ok.Should().NotBeNull();
  }

  // ---------------- ASSIGN ROLE ----------------
  [Fact]
  public async Task AssignRole_UserNotFound_ReturnsNotFound()
  {
    var userManager = MockUserManager();
    var signInManager = MockSignInManager(userManager.Object);

    userManager.Setup(u => u.FindByIdAsync(It.IsAny<string>()))
        .ReturnsAsync((User)null);

    var controller = CreateController(userManager, signInManager);

    var result = await controller.AssignRole(new AssignRoleDto
    {
      UserId = "1",
      Role = "Admin"
    });

    result.Should().BeOfType<NotFoundObjectResult>();
  }

  [Fact]
  public async Task AssignRole_Success_ReturnsOk()
  {
    var userManager = MockUserManager();
    var signInManager = MockSignInManager(userManager.Object);

    var user = new User { Id = 1, Email = "test@test.com" };

    userManager.Setup(u => u.FindByIdAsync("1"))
        .ReturnsAsync(user);

    userManager.Setup(u => u.GetRolesAsync(user))
        .ReturnsAsync(new List<string> { "User" });

    userManager.Setup(u => u.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>()))
        .ReturnsAsync(IdentityResult.Success);

    userManager.Setup(u => u.AddToRoleAsync(user, "Admin"))
        .ReturnsAsync(IdentityResult.Success);

    var controller = CreateController(userManager, signInManager);

    var result = await controller.AssignRole(new AssignRoleDto
    {
      UserId = "1",
      Role = "Admin"
    });

    result.Should().BeOfType<OkObjectResult>();
  }
}
