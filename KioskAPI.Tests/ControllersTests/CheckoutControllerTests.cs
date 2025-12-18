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
  using Microsoft.EntityFrameworkCore.Diagnostics;
  using Xunit;

  public class CheckoutControllerTests
  {
    private AppDbContext GetInMemoryDb()
    {
      var options = new DbContextOptionsBuilder<AppDbContext>()
          .UseInMemoryDatabase(Guid.NewGuid().ToString())
          .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
          .Options;
      return new AppDbContext(options);
    }

    private CheckoutController GetController(AppDbContext context, int userId)
    {
      var controller = new CheckoutController(context);
      controller.ControllerContext = new ControllerContext
      {
        HttpContext = new DefaultHttpContext
        {
          User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
              {
                        new Claim(ClaimTypes.NameIdentifier, userId.ToString())
              }, "mock"))
        }
      };
      return controller;
    }

    [Fact]
    public async Task GetCheckoutSummary_ReturnsOk_WhenCartHasItems()
    {
      using var context = GetInMemoryDb();
      int userId = 1;

      var cart = new Cart
      {
        CartId = 1,
        UserId = userId,
        IsCheckedOut = false,
        CartItems = new List<CartItem>
        {
            new CartItem
            {
                CartItemId = 1,
                ProductId = 1,
                Quantity = 2,
                UnitPrice = 50,
                Product = new Product
                {
                    ProductId = 1,
                    Name = "Test Product",
                    Category = "Test",
                    Description = "Test Desc",
                    Price = 50,
                    Quantity = 10
                }
            }
        }
      };

      context.Carts.Add(cart);
      await context.SaveChangesAsync();

      var controller = GetController(context, userId);

      var result = await controller.GetCheckoutSummary();
      var okResult = result.Should().BeOfType<OkObjectResult>().Subject;

      var value = okResult.Value;

      var cartId = (int)value.GetType()
          .GetProperty("cartId")!
          .GetValue(value)!;

      var itemsTotal = (decimal)value.GetType()
          .GetProperty("itemsTotal")!
          .GetValue(value)!;

      cartId.Should().Be(1);
      itemsTotal.Should().Be(100);
    }

    [Fact]
    public async Task GetCheckoutSummary_ReturnsNotFound_WhenCartEmpty()
    {
      using var context = GetInMemoryDb();
      int userId = 1;

      var controller = GetController(context, userId);

      var result = await controller.GetCheckoutSummary();
      var notFoundResult = result as NotFoundObjectResult;
      notFoundResult.Should().NotBeNull();
    }

    [Fact]
    public async Task Checkout_ReturnsOk_WhenSufficientBalance()
    {
      using var context = GetInMemoryDb();
      int userId = 1;

      var product = new Product
      {
        ProductId = 1,
        Name = "Test Product",
        Category = "Test",
        Description = "Desc",
        Price = 50,
        Quantity = 10
      };
      context.Products.Add(product);

      var cart = new Cart
      {
        CartId = 1,
        UserId = userId,
        IsCheckedOut = false,
        ExpiresAt = DateTime.UtcNow.AddHours(1),
        CartItems = new List<CartItem>
                {
                    new CartItem
                    {
                        CartItemId = 1,
                        ProductId = 1,
                        Quantity = 2,
                        UnitPrice = 50,
                        Product = product
                    }
                }
      };
      context.Carts.Add(cart);

      var account = new Account
      {
        AccountId = 1,
        UserId = userId,
        Balance = 200
      };
      context.Accounts.Add(account);

      await context.SaveChangesAsync();

      var controller = GetController(context, userId);

      var dto = new CheckoutRequestDto
      {
        DeliveryMethod = "pickup"
      };

      var result = await controller.Checkout(dto);
      var okResult = result as OkObjectResult;
      okResult.Should().NotBeNull();

      var response = okResult.Value as CheckoutResponseDto;
      response.Should().NotBeNull();
      response.TotalAmount.Should().Be(100);
      response.DeliveryFee.Should().Be(0);
    }

    [Fact]
    public async Task Checkout_ReturnsBadRequest_WhenInsufficientBalance()
    {
      using var context = GetInMemoryDb();
      int userId = 1;

      var product = new Product
      {
        ProductId = 1,
        Name = "Test Product",
        Category = "Test",
        Description = "Desc",
        Price = 50,
        Quantity = 10
      };
      context.Products.Add(product);

      var cart = new Cart
      {
        CartId = 1,
        UserId = userId,
        IsCheckedOut = false,
        ExpiresAt = DateTime.UtcNow.AddHours(1),
        CartItems = new List<CartItem>
                {
                    new CartItem
                    {
                        CartItemId = 1,
                        ProductId = 1,
                        Quantity = 2,
                        UnitPrice = 50,
                        Product = product
                    }
                }
      };
      context.Carts.Add(cart);

      var account = new Account
      {
        AccountId = 1,
        UserId = userId,
        Balance = 50 // insufficient for total 100
      };
      context.Accounts.Add(account);

      await context.SaveChangesAsync();

      var controller = GetController(context, userId);

      var dto = new CheckoutRequestDto
      {
        DeliveryMethod = "pickup"
      };

      var result = await controller.Checkout(dto);
      var badRequest = result as BadRequestObjectResult;
      badRequest.Should().NotBeNull();
    }
  }
}
