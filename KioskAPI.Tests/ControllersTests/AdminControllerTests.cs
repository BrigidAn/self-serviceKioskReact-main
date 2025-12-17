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
      _context = TestDbFactory.Create(Guid.NewGuid().ToString());
      _userManagerMock = UserManagerMock.Create<User>();

      _controller = new AdminController(
          _userManagerMock.Object,
          _context,
          NullLogger<AdminController>.Instance
      );
    }

    // ===================== HELPER =====================

    private static T GetProperty<T>(object obj, string propertyName)
    {
      var prop = obj.GetType().GetProperty(propertyName);
      prop.Should().NotBeNull($"Property '{propertyName}' should exist");
      return (T)prop.GetValue(obj);
    }

    // ===================== USERS =====================

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

      _context.Users.Add(user);
      _context.Accounts.Add(new Account { UserId = 1, Balance = 500 });
      await _context.SaveChangesAsync();

      _userManagerMock.Setup(u => u.Users)
          .Returns(_context.Users);

      _userManagerMock.Setup(u => u.GetRolesAsync(user))
          .ReturnsAsync(new List<string> { "Admin" });

      var result = await _controller.GetAllUsers();

      var ok = result.Should().BeOfType<OkObjectResult>().Subject;
      var value = ok.Value;

      GetProperty<int>(value, "total").Should().Be(1);
    }

    // ===================== PRODUCTS =====================

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

      _context.Suppliers.Add(supplier);
      _context.Products.Add(product);
      await _context.SaveChangesAsync();

      var result = await _controller.GetAllProducts();

      var ok = result.Should().BeOfType<OkObjectResult>().Subject;
      var value = ok.Value;

      GetProperty<int>(value, "total").Should().Be(1);
    }

    // ===================== ORDERS =====================

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

      _context.Users.Add(user);
      _context.Orders.Add(order);
      await _context.SaveChangesAsync();

      var result = await _controller.GetAllOrders();

      var ok = result.Should().BeOfType<OkObjectResult>().Subject;
      var value = ok.Value;

      GetProperty<int>(value, "total").Should().Be(1);
    }

    // ===================== TRANSACTIONS =====================

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

      _context.Users.Add(user);
      _context.Accounts.Add(account);
      _context.Transactions.Add(transaction);
      await _context.SaveChangesAsync();

      var result = await _controller.GetAllTransactions();

      var ok = result.Should().BeOfType<OkObjectResult>().Subject;
      var value = ok.Value;

      GetProperty<int>(value, "total").Should().Be(1);
    }

    // ===================== TOP UP =====================

    [Fact]
    public async Task TopUpUser_AddsBalance_AndCreatesTransaction()
    {
      var user = new User { Id = 1, UserName = "user@test.com" };
      _context.Users.Add(user);
      await _context.SaveChangesAsync();

      var dto = new AdminTopUpDo
      {
        UserId = 1,
        Amount = 200,
        Description = "TopUp"
      };

      var result = await _controller.TopUpUser(dto);

      result.Should().BeOfType<OkObjectResult>();

      var account = await _context.Accounts.FirstAsync();
      account.Balance.Should().Be(200);

      _context.Transactions.Should().HaveCount(1);
    }

    // ===================== CART =====================

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

      _context.Users.Add(user);
      _context.Products.Add(product);
      await _context.SaveChangesAsync();

      var dto = new AdminAddToCartDto
      {
        UserId = 1,
        ProductId = 1,
        Quantity = 2
      };

      var result = await _controller.AddToUserCart(dto);

      result.Should().BeOfType<OkObjectResult>();
      product.Quantity.Should().Be(3);
      _context.CartItems.Should().HaveCount(1);
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

      _context.Products.Add(product);
      _context.Carts.Add(cart);
      await _context.SaveChangesAsync();

      var result = await _controller.GetUserCartSummary(1);

      var ok = result.Should().BeOfType<OkObjectResult>().Subject;
      var value = ok.Value;

      GetProperty<decimal>(value, "itemsTotal").Should().Be(20);
    }

    // ===================== CHECKOUT =====================

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

      _context.Products.Add(product);
      _context.Carts.Add(cart);
      await _context.SaveChangesAsync();

      var result = await _controller.CheckoutUserCart(1);

      result.Should().BeOfType<OkObjectResult>();
      _context.Orders.Should().HaveCount(1);
      _context.OrderItems.Should().HaveCount(1);
    }
  }
}
