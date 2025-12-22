namespace KioskAPI.Tests.ControllersTests
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Security.Claims;
  using System.Threading.Tasks;
  using FluentAssertions;
  using KioskAPI.Controllers;
  using KioskAPI.Data;
  using KioskAPI.Dtos;
  using KioskAPI.Models;
  using Microsoft.AspNetCore.Http;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.EntityFrameworkCore;
  using Xunit;

  public class UserControllerTests
  {
    private static AppDbContext GetInMemoryDb()
    {
      var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .ConfigureWarnings(w =>
          w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
        .Options;

      return new AppDbContext(options);
    }

    private static UserController GetController(
      AppDbContext context,
      int userId,
      bool isAdmin = false)
    {
      var controller = new UserController(context);

      var claims = new List<Claim>
      {
        new Claim(ClaimTypes.NameIdentifier, userId.ToString())
      };

      if (isAdmin)
      {
        claims.Add(new Claim(ClaimTypes.Role, "Admin"));
      }

      var identity = new ClaimsIdentity(claims, "TestAuth");
      var principal = new ClaimsPrincipal(identity);

      controller.ControllerContext = new ControllerContext
      {
        HttpContext = new DefaultHttpContext { User = principal }
      };

      return controller;
    }

    [Fact]
    public async Task GetProfile_ReturnsOk_WhenUserExists()
    {
      using var context = GetInMemoryDb();

      context.Users.Add(new User
      {
        Id = 1,
        Name = "Test User",
        Email = "test@test.com",
        CreatedAt = DateTime.UtcNow
      });

      await context.SaveChangesAsync();

      var controller = GetController(context, userId: 1);

      var result = await controller.GetProfile();

      result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetProfile_ReturnsNotFound_WhenUserDoesNotExist()
    {
      using var context = GetInMemoryDb();

      var controller = GetController(context, userId: 1);

      var result = await controller.GetProfile();

      result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetAccount_ReturnsOk_WhenAccountExists()
    {
      using var context = GetInMemoryDb();

      var user = new User
      {
        Id = 1,
        Name = "User",
        Email = "user@test.com",
        CreatedAt = DateTime.UtcNow
      };

      var account = new Account
      {
        AccountId = 1,
        User = user,
        Balance = 100,
        LastUpdated = DateTime.UtcNow,
        Transactions = new List<Transaction>()
      };

      context.Users.Add(user);
      context.Accounts.Add(account);

      await context.SaveChangesAsync();

      var controller = GetController(context, userId: 1);

      var result = await controller.GetAccount();

      result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAccount_ReturnsNotFound_WhenAccountMissing()
    {
      using var context = GetInMemoryDb();

      context.Users.Add(new User
      {
        Id = 1,
        Name = "User",
        Email = "user@test.com",
        CreatedAt = DateTime.UtcNow
      });

      await context.SaveChangesAsync();

      var controller = GetController(context, userId: 1);

      var result = await controller.GetAccount();

      result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task TopUp_IncreasesBalance_AndCreatesTransaction()
    {
      using var context = GetInMemoryDb();

      var user = new User
      {
        Id = 1,
        Name = "User",
        Email = "user@test.com",
        CreatedAt = DateTime.UtcNow
      };

      var account = new Account
      {
        AccountId = 1,
        User = user,
        Balance = 100,
        LastUpdated = DateTime.UtcNow
      };

      context.Users.Add(user);
      context.Accounts.Add(account);

      await context.SaveChangesAsync();

      var controller = GetController(context, userId: 1);

      var dto = new TopUpDto { Amount = 50 };

      var result = await controller.TopUp(dto);

      result.Should().BeOfType<OkObjectResult>();

      var updatedAccount = await context.Accounts.FirstAsync();
      updatedAccount.Balance.Should().Be(150);

      context.Transactions.Should().HaveCount(1);
    }

    [Fact]
    public async Task TopUp_ReturnsBadRequest_WhenAmountInvalid()
    {
      using var context = GetInMemoryDb();

      var controller = GetController(context, userId: 1);

      var dto = new TopUpDto { Amount = 0 };

      var result = await controller.TopUp(dto);

      result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task TopUp_ReturnsBadRequest_WhenBalanceLimitExceeded()
    {
      using var context = GetInMemoryDb();

      var user = new User { Id = 1, Name = "User", Email = "user@test.com" };
      var account = new Account
      {
        AccountId = 1,
        User = user,
        Balance = 99950
      };

      context.Users.Add(user);
      context.Accounts.Add(account);
      await context.SaveChangesAsync();

      var controller = GetController(context, userId: 1);

      var dto = new TopUpDto { Amount = 100 };

      var result = await controller.TopUp(dto);

      result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetUserProfileAdmin_ReturnsOk_ForAdmin()
    {
      using var context = GetInMemoryDb();

      context.Users.Add(new User
      {
        Id = 2,
        Name = "Admin User",
        Email = "admin@test.com",
        CreatedAt = DateTime.UtcNow
      });

      await context.SaveChangesAsync();

      var controller = GetController(context, userId: 1, isAdmin: true);

      var result = await controller.GetUserProfileAdmin(2);

      result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAccountAdmin_ReturnsOk_WhenAccountExists()
    {
      using var context = GetInMemoryDb();

      var user = new User
      {
        Id = 2,
        Name = "User",
        Email = "user@test.com",
        CreatedAt = DateTime.UtcNow
      };

      var account = new Account
      {
        AccountId = 1,
        User = user,
        Balance = 300
      };

      context.Users.Add(user);
      context.Accounts.Add(account);
      await context.SaveChangesAsync();

      var controller = GetController(context, userId: 1, isAdmin: true);

      var result = await controller.GetAccountAdmin(2);

      result.Should().BeOfType<OkObjectResult>();
    }
  }
}
