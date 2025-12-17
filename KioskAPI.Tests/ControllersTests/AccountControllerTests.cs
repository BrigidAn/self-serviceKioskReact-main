using Xunit;
using Moq;
using KioskAPI.Controllers;
using KioskAPI.interfaces;
using KioskAPI.Models;
using Microsoft.AspNetCore.Mvc;
using FluentAssertions;
using KioskAPI.Dtos;

public class AccountControllerTests
{
  private readonly Mock<IAccountRepository> _repo;
  private readonly AccountController _controller;

  public AccountControllerTests()
  {
    this._repo = new Mock<IAccountRepository>();
    this._controller = new AccountController(this._repo.Object);
    this._controller.ControllerContext = TestUserHelper.GetControllerContext(1);
  }

  [Fact]
  public async Task GetBalance_Returns_Account()
  {
    this._repo.Setup(r => r.GetAccountByUserIdAsync(1))
         .ReturnsAsync(new Account { Balance = 100 });

    var result = await this._controller.GetBalance();

    var ok = Assert.IsType<OkObjectResult>(result);
    Assert.Equal(100, ((dynamic)ok.Value).Balance);
  }

  [Fact]
  public async Task GetBalance_AccountNotFound_ReturnsNotFound()
  {
    _repo.Setup(r => r.GetAccountByUserIdAsync(1))
        .ReturnsAsync((Account)null);

    var result = await _controller.GetBalance();

    result.Should().BeOfType<NotFoundObjectResult>();
  }

  [Fact]
  public async Task TopUp_ValidAmount_ReturnsUpdatedBalance()
  {
    var account = new Account { AccountId = 1, Balance = 200 };

    _repo.Setup(r => r.GetAccountByUserIdAsync(1))
        .ReturnsAsync(account);

    _repo.Setup(r => r.UpdateBalanceAsync(1, 300))
        .Returns(Task.CompletedTask);

    var result = await _controller.TopUp(new TopUpDto
    {
      Amount = 300
    });

    var ok = result as OkObjectResult;
    ok.Should().NotBeNull();
  }

  [Fact]
  public async Task TopUp_AmountExceedsLimit_ReturnsBadRequest()
  {
    var result = await _controller.TopUp(new TopUpDto
    {
      Amount = 2000
    });

    result.Should().BeOfType<BadRequestObjectResult>();
  }

  [Fact]
  public async Task GetTransactions_ReturnsUserTransactions()
  {
    var transactions = new List<Transaction>
        {
            new Transaction
            {
                TransactionId = 1,
                Description = "Top-up",
                TotalAmount = 100,
                Type = "Credit"
            }
        };

    _repo.Setup(r => r.GetTransactionsAsync(1))
        .ReturnsAsync(transactions);

    var result = await _controller.GetTransactions();

    var ok = result as OkObjectResult;
    ok.Should().NotBeNull();
  }

  [Fact]
  public async Task MyAccount_ReturnsAccountAndTransactions()
  {
    var account = new Account
    {
      AccountId = 1,
      Balance = 500
    };

    var transactions = new List<Transaction>
        {
            new Transaction
            {
                TransactionId = 1,
                Description = "Top-up",
                TotalAmount = 200,
                Type = "Credit"
            }
        };

    _repo.Setup(r => r.GetAccountByUserIdAsync(1))
        .ReturnsAsync(account);

    _repo.Setup(r => r.GetTransactionsAsync(1))
        .ReturnsAsync(transactions);

    var result = await _controller.GetCurrentUser();

    var ok = result as OkObjectResult;
    ok.Should().NotBeNull();
  }
}