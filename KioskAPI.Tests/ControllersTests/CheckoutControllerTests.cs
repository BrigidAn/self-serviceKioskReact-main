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
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

public class CheckoutControllerTests
{
  private AppDbContext GetInMemoryDbContext()
  {
    var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning)) // Ignore transactions warning
        .Options;
    return new AppDbContext(options);
  }

  private CheckoutController CreateControllerWithUser(AppDbContext context, int userId, bool isAdmin = false)
  {
    var controller = new CheckoutController(context);

    // Mock HttpContext and ClaimsPrincipal
    var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
    {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Role, isAdmin ? "Admin" : "User")
    }, "mock"));

    controller.ControllerContext = new ControllerContext
    {
      HttpContext = new DefaultHttpContext { User = user }
    };

    return controller;
  }

  [Fact]
  public async Task GetCheckoutSummary_ReturnsOk_WhenCartHasItems()
  {
    // Arrange
    var context = GetInMemoryDbContext();
    var userId = 1;

    var product = new Product { ProductId = 1, Name = "Product A", Quantity = 10, Price = 50, ImageUrl = "img.jpg", Category = "category", Description = "description", SupplierId = 1 };
    context.Products.Add(product);
    await context.SaveChangesAsync();

    var cart = new Cart
    {
      CartId = 1,
      UserId = userId,
      IsCheckedOut = false,
      CartItems = new List<CartItem>
            {
                new CartItem { CartItemId = 1, ProductId = 1, UnitPrice = 50, Quantity = 2, Product = product }
            }
    };
    context.Carts.Add(cart);
    await context.SaveChangesAsync();

    var controller = CreateControllerWithUser(context, userId);

    // Act
    var result = await controller.GetCheckoutSummary();

    // Assert
    var okResult = result as OkObjectResult;
    okResult.Should().NotBeNull();

    var summary = okResult.Value as dynamic;
    ((int)summary.cartId).Should().Be(cart.CartId);
    ((decimal)summary.itemsTotal).Should().Be(100);
    ((IEnumerable<dynamic>)summary.items).Should().HaveCount(1);
  }

  [Fact]
  public async Task GetCheckoutSummary_ReturnsNotFound_WhenCartEmpty()
  {
    var context = GetInMemoryDbContext();
    var controller = CreateControllerWithUser(context, userId: 1);

    var result = await controller.GetCheckoutSummary();

    result.Should().BeOfType<NotFoundObjectResult>();
  }

  [Fact]
  public async Task Checkout_ReturnsBadRequest_WhenInsufficientBalance()
  {
    var context = GetInMemoryDbContext();
    var userId = 1;

    // Account with low balance
    var account = new Account { AccountId = 1, UserId = userId, Balance = 50 };
    context.Accounts.Add(account);

    var product = new Product { ProductId = 1, Name = "Product A", Quantity = 10, Price = 100, ImageUrl = "img.jpg", Category = "category", Description = "description", SupplierId = 1 };
    context.Products.Add(product);

    var cart = new Cart
    {
      CartId = 1,
      UserId = userId,
      IsCheckedOut = false,
      ExpiresAt = DateTime.UtcNow.AddMinutes(10),
      CartItems = new List<CartItem>
            {
                new CartItem { CartItemId = 1, ProductId = 1, UnitPrice = 100, Quantity = 1, Product = product }
            }
    };
    context.Carts.Add(cart);

    await context.SaveChangesAsync();

    var controller = CreateControllerWithUser(context, userId);

    var dto = new CheckoutRequestDto { DeliveryMethod = "Delivery" };
    var result = await controller.Checkout(dto);

    var badRequest = result as BadRequestObjectResult;
    badRequest.Should().NotBeNull();
    var value = badRequest.Value as dynamic;
    ((decimal)value.remainingAmount).Should().Be(130); // 100 + 80 - 50
  }

  [Fact]
  public async Task Checkout_ReturnsOk_WhenSufficientBalance()
  {
    var context = GetInMemoryDbContext();
    var userId = 1;

    // Account with enough balance
    var account = new Account { AccountId = 1, UserId = userId, Balance = 500 };
    context.Accounts.Add(account);

    var product = new Product { ProductId = 1, Name = "Product A", Quantity = 10, Price = 100, ImageUrl = "img.jpg", Category = "category", Description = "description", SupplierId = 1 };
    context.Products.Add(product);

    var cart = new Cart
    {
      CartId = 1,
      UserId = userId,
      IsCheckedOut = false,
      ExpiresAt = DateTime.UtcNow.AddMinutes(10),
      CartItems = new List<CartItem>
            {
                new CartItem { CartItemId = 1, ProductId = 1, UnitPrice = 100, Quantity = 2, Product = product }
            }
    };
    context.Carts.Add(cart);

    await context.SaveChangesAsync();

    var controller = CreateControllerWithUser(context, userId);

    var dto = new CheckoutRequestDto { DeliveryMethod = "Delivery" };
    var result = await controller.Checkout(dto);

    var okResult = result as OkObjectResult;
    okResult.Should().NotBeNull();

    var checkoutResponse = okResult.Value as CheckoutResponseDto;
    checkoutResponse.Should().NotBeNull();
    checkoutResponse.Message.Should().Be("Checkout successful");
    checkoutResponse.TotalAmount.Should().Be(280); // 2*100 + 80 delivery
    checkoutResponse.DeliveryFee.Should().Be(80);
  }
}
