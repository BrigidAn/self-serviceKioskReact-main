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

  public class TransactionControllerTests
  {
    private AppDbContext GetInMemoryDb()
    {
      var options = new DbContextOptionsBuilder<AppDbContext>()
          .UseInMemoryDatabase(Guid.NewGuid().ToString())
          .Options;
      return new AppDbContext(options);
    }

    private TransactionController GetController(AppDbContext context, int? userId = null, bool isAdmin = false)
    {
      var controller = new TransactionController(context);

      var claims = new List<Claim>();
      if (userId.HasValue)
      {
        claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()));
      }

      if (isAdmin)
      {
        claims.Add(new Claim(ClaimTypes.Role, "Admin"));
      }

      controller.ControllerContext = new ControllerContext
      {
        HttpContext = new DefaultHttpContext
        {
          User = new ClaimsPrincipal(new ClaimsIdentity(claims))
        }
      };
      return controller;
    }

    [Fact]
    public async Task GetAllTransactions_ReturnsOk_WithTransactions_WhenAdmin()
    {
      using var context = this.GetInMemoryDb();
      var user = new User { Id = 1, Name = "John" };
      var account = new Account { AccountId = 1, UserId = 1, User = user, Balance = 100 };
      var transaction = new Transaction { TransactionId = 1, AccountId = 1, Account = account, Type = "Credit", TotalAmount = 50, CreatedAt = DateTime.UtcNow };
      context.Users.Add(user);
      context.Accounts.Add(account);
      context.Transactions.Add(transaction);
      await context.SaveChangesAsync();

      var controller = this.GetController(context, userId: 1, isAdmin: true);

      var result = await controller.GetAllTransactions();
      var ok = result as OkObjectResult;
      ok.Should().NotBeNull();

      var transactions = ok.Value as IEnumerable<dynamic>;
      transactions.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetMyTransactions_ReturnsOk_WithUserTransactions()
    {
      using var context = this.GetInMemoryDb();
      var user = new User { Id = 2, Name = "Alice" };
      var account = new Account { AccountId = 2, UserId = 2, User = user, Balance = 100 };
      var transaction = new Transaction { TransactionId = 2, AccountId = 2, Account = account, Type = "Debit", TotalAmount = 20, CreatedAt = DateTime.UtcNow };
      context.Users.Add(user);
      context.Accounts.Add(account);
      context.Transactions.Add(transaction);
      await context.SaveChangesAsync();

      var controller = this.GetController(context, userId: 2);

      var result = await controller.GetMyTransactions();
      var ok = result as OkObjectResult;
      ok.Should().NotBeNull();

      var transactions = ok.Value as IEnumerable<dynamic>;
      transactions.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateTransaction_AddsCreditTransaction()
    {
      using var context = this.GetInMemoryDb();

      var account = new Account
      {
        AccountId = 1,
        UserId = 1,
        Balance = 100
      };
      context.Accounts.Add(account);
      await context.SaveChangesAsync();

      var controller = this.GetController(context, userId: 1);

      var dto = new TransactionDto
      {
        AccountId = 1,
        Type = "credit",
        TotalAmount = 50,
        Description = "Deposit"
      };

      var result = await controller.CreateTransaction(dto);

      result.Should().BeOfType<OkObjectResult>();

      var updatedAccount = await context.Accounts.FindAsync(1);
      updatedAccount!.Balance.Should().Be(150);

      context.Transactions.Should().HaveCount(1);
      context.Transactions.First().Type.Should().Be("credit");
    }

    [Fact]
    public async Task CreateTransaction_AddsDebitTransaction_WhenSufficientBalance()
    {
      using var context = this.GetInMemoryDb();

      var account = new Account
      {
        AccountId = 1,
        UserId = 1,
        Balance = 100
      };
      context.Accounts.Add(account);
      await context.SaveChangesAsync();

      var controller = this.GetController(context, userId: 1);

      var dto = new TransactionDto
      {
        AccountId = 1,
        Type = "debit",
        TotalAmount = 60,
        Description = "Payment"
      };

      var result = await controller.CreateTransaction(dto);

      result.Should().BeOfType<OkObjectResult>();

      var updatedAccount = await context.Accounts.FindAsync(1);
      updatedAccount!.Balance.Should().Be(40);

      context.Transactions.Should().HaveCount(1);
      context.Transactions.First().Type.Should().Be("debit");
    }

    [Fact]
    public async Task CreateTransaction_ReturnsBadRequest_WhenDebitInsufficientBalance()
    {
      using var context = this.GetInMemoryDb();
      var account = new Account { AccountId = 1, UserId = 1, Balance = 30 };
      context.Accounts.Add(account);
      await context.SaveChangesAsync();

      var controller = this.GetController(context, userId: 1);

      var dto = new TransactionDto { AccountId = 1, Type = "debit", TotalAmount = 50, Description = "Payment" };
      var result = await controller.CreateTransaction(dto);

      result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task DeleteTransaction_AdjustsAccountBalance()
    {
      using var context = this.GetInMemoryDb();
      var user = new User { Id = 1, Name = "John" };
      var account = new Account { AccountId = 1, UserId = 1, User = user, Balance = 100 };
      var transaction = new Transaction { TransactionId = 1, AccountId = 1, Account = account, Type = "credit", TotalAmount = 50, CreatedAt = DateTime.UtcNow };
      context.Users.Add(user);
      context.Accounts.Add(account);
      context.Transactions.Add(transaction);
      await context.SaveChangesAsync();

      var controller = this.GetController(context, userId: 1, isAdmin: true);

      var result = await controller.DeleteTransaction(1);
      var ok = result as OkObjectResult;
      ok.Should().NotBeNull();

      var updatedAccount = await context.Accounts.FindAsync(1);
      updatedAccount.Balance.Should().Be(100 - 50);
    }

    [Fact]
    public async Task DeleteTransaction_ReturnsNotFound_WhenTransactionDoesNotExist()
    {
      using var context = this.GetInMemoryDb();
      var controller = this.GetController(context, userId: 1, isAdmin: true);

      var result = await controller.DeleteTransaction(999);
      result.Should().BeOfType<NotFoundObjectResult>();
    }
  }
}
