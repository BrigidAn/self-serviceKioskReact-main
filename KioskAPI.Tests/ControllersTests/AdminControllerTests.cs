namespace KioskAPI.Tests.ControllersTests
{
  using Xunit;
  using FluentAssertions;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.Extensions.Logging.Abstractions;
  using Microsoft.EntityFrameworkCore;
  using Moq;
  using System;
  using System.Linq;
  using System.Threading.Tasks;
  using System.Collections.Generic;

  using KioskAPI.Controllers;
  using KioskAPI.Data;
  using KioskAPI.Models;
  using KioskAPI.Dtos;
  using KioskAPI.Tests.Helpers;
  using Microsoft.AspNetCore.Identity;

  public class AdminControllerTests
  {
    private readonly AppDbContext _context;
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly AdminController _controller;

    public AdminControllerTests()
    {
      this._context = TestDbFactory.Create(Guid.NewGuid().ToString());
      this._userManagerMock = UserManagerMock.Create<User>();

      this._controller = new AdminController(
          this._userManagerMock.Object,
          this._context,
          NullLogger<AdminController>.Instance
      );
    }

    private static T GetProperty<T>(object obj, string propertyName)
    {
      var prop = obj.GetType().GetProperty(propertyName);
      prop.Should().NotBeNull($"Property '{propertyName}' should exist");
      return (T)prop.GetValue(obj);
    }

    [Fact]
    public async Task GetAllUsers_ReturnsOk_WithUsers()
    {
      var user = new User
      {
        Id = 1,
        UserName = "admin@test.com",
        Email = "admin@test.com",
        CreatedAt = DateTime.UtcNow
      };

      this._context.Users.Add(user);
      this._context.Accounts.Add(new Account { UserId = 1, Balance = 500 });
      await this._context.SaveChangesAsync();

      this._userManagerMock.Setup(u => u.Users)
          .Returns(this._context.Users);

      this._userManagerMock.Setup(u => u.GetRolesAsync(user))
          .ReturnsAsync(new List<string> { "Admin" });

      var result = await this._controller.GetAllUsers();

      var ok = result.Should().BeOfType<OkObjectResult>().Subject;
      var value = ok.Value;

      GetProperty<int>(value, "total").Should().Be(1);
    }

    [Fact]
    public async Task GetAllProducts_ReturnsProducts()
    {
      var supplier = new Supplier { SupplierId = 1, Name = "Supplier" };

      var product = new Product
      {
        ProductId = 1,
        Name = "Coke",
        Description = "Cold drink",
        Category = "Drinks",
        Price = 15,
        Quantity = 20,
        Supplier = supplier
      };

      this._context.Suppliers.Add(supplier);
      this._context.Products.Add(product);
      await this._context.SaveChangesAsync();

      var result = await this._controller.GetAllProducts();

      var ok = result.Should().BeOfType<OkObjectResult>().Subject;
      var value = ok.Value;

      GetProperty<int>(value, "total").Should().Be(1);
    }

    [Fact]
    public async Task GetAllOrders_ReturnsOrders()
    {
      var user = new User { Id = 1, Name = "John Doe" };

      var order = new Order
      {
        User = user,
        OrderDate = DateTime.UtcNow,
        Status = "Pending",
        PaymentStatus = "Pending",
        TotalAmount = 100
      };

      this._context.Users.Add(user);
      this._context.Orders.Add(order);
      await this._context.SaveChangesAsync();

      var result = await this._controller.GetAllOrders();

      var ok = result.Should().BeOfType<OkObjectResult>().Subject;
      var value = ok.Value;

      GetProperty<int>(value, "total").Should().Be(1);
    }

    [Fact]
    public async Task GetAllTransactions_ReturnsTransactions()
    {
      var user = new User { Id = 1, Name = "Jane" };
      var account = new Account { AccountId = 1, User = user, Balance = 300 };

      var transaction = new Transaction
      {
        Account = account,
        Type = "TopUp",
        TotalAmount = 100,
        Description = "Admin TopUp",
        CreatedAt = DateTime.UtcNow
      };

      this._context.Users.Add(user);
      this._context.Accounts.Add(account);
      this._context.Transactions.Add(transaction);
      await this._context.SaveChangesAsync();

      var result = await this._controller.GetAllTransactions();

      var ok = result.Should().BeOfType<OkObjectResult>().Subject;
      var value = ok.Value;

      GetProperty<int>(value, "total").Should().Be(1);
    }

    [Fact]
    public async Task TopUpUser_AddsBalance_AndCreatesTransaction()
    {
      var user = new User { Id = 1, UserName = "user@test.com" };
      this._context.Users.Add(user);
      await this._context.SaveChangesAsync();

      var dto = new AdminTopUpDo
      {
        UserId = 1,
        Amount = 200,
        Description = "TopUp"
      };

      var result = await this._controller.TopUpUser(dto);

      result.Should().BeOfType<OkObjectResult>();

      var account = await this._context.Accounts.FirstAsync();
      account.Balance.Should().Be(200);

      this._context.Transactions.Should().HaveCount(1);
    }

    [Fact]
    public async Task AddToUserCart_AddsItemAndReducesStock()
    {
      var user = new User { Id = 1 };

      var product = new Product
      {
        ProductId = 1,
        Name = "Snack",
        Description = "Chips",
        Category = "Food",
        Price = 10,
        Quantity = 5
      };

      this._context.Users.Add(user);
      this._context.Products.Add(product);
      await this._context.SaveChangesAsync();

      var dto = new AdminAddToCartDto
      {
        UserId = 1,
        ProductId = 1,
        Quantity = 2
      };

      var result = await this._controller.AddToUserCart(dto);

      result.Should().BeOfType<OkObjectResult>();
      product.Quantity.Should().Be(3);
      this._context.CartItems.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetUserCartSummary_ReturnsSummary()
    {
      var product = new Product
      {
        ProductId = 1,
        Name = "Juice",
        Description = "Orange",
        Category = "Drinks",
        Price = 10,
        Quantity = 10
      };

      var cart = new Cart
      {
        UserId = 1,
        CartItems = new List<CartItem>
        {
          new CartItem
          {
            Product = product,
            Quantity = 2,
            UnitPrice = 10
          }
        }
      };

      this._context.Products.Add(product);
      this._context.Carts.Add(cart);
      await this._context.SaveChangesAsync();

      var result = await this._controller.GetUserCartSummary(1);

      var ok = result.Should().BeOfType<OkObjectResult>().Subject;
      var value = ok.Value;

      GetProperty<decimal>(value, "itemsTotal").Should().Be(20);
    }

    [Fact]
    public async Task CheckoutUserCart_CreatesOrder()
    {
      var product = new Product
      {
        ProductId = 1,
        Name = "Water",
        Description = "Bottle",
        Category = "Drinks",
        Price = 5,
        Quantity = 10
      };

      var cart = new Cart
      {
        UserId = 1,
        CartItems = new List<CartItem>
        {
          new CartItem
          {
            Product = product,
            Quantity = 2,
            UnitPrice = 5
          }
        }
      };

      this._context.Products.Add(product);
      this._context.Carts.Add(cart);
      await this._context.SaveChangesAsync();

      var result = await this._controller.CheckoutUserCart(1);

      result.Should().BeOfType<OkObjectResult>();
      this._context.Orders.Should().HaveCount(1);
      this._context.OrderItems.Should().HaveCount(1);
    }
  }
}
