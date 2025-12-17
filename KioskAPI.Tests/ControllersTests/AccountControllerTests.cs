using Xunit;
using Moq;
using KioskAPI.Controllers;
using KioskAPI.interfaces;
using KioskAPI.Models;
using Microsoft.AspNetCore.Mvc;

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
}
