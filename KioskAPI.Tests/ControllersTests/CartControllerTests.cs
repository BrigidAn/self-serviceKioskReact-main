using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using KioskAPI.Controllers;
using KioskAPI.Data;
using KioskAPI.Dtos;
using KioskAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

public class CartControllerTests
{
  private AppDbContext GetInMemoryDb()
  {
    var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .ConfigureWarnings(w =>
            w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
        .Options;

    return new AppDbContext(options);
  }

  private CartController GetController(AppDbContext context, int userId = 1)
  {
    var mockLogger = new Mock<ILogger<CartController>>();
    var controller = new CartController(context, mockLogger.Object);

    var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
    {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        }, "mock"));

    controller.ControllerContext = new ControllerContext
    {
      HttpContext = new DefaultHttpContext { User = user }
    };

    return controller;
  }

  [Fact]
  public async Task AddToCart_ProductDoesNotExist_ReturnsBadRequest()
  {
    var db = this.GetInMemoryDb();
    var controller = this.GetController(db);

    var result = await controller.AddToCart(new AddToCartDto
    {
      ProductId = 999,
      Quantity = 1
    });

    result.Should().BeOfType<BadRequestObjectResult>();
  }

  [Fact]
  public async Task AddToCart_ValidProduct_AddsItemToCart()
  {
    var db = this.GetInMemoryDb();

    db.Products.Add(new Product
    {
      ProductId = 1,
      Name = "Test Product",
      Description = "Test description",
      Category = "Snacks",
      Price = 10,
      Quantity = 5
    });

    await db.SaveChangesAsync();

    var controller = this.GetController(db);

    var result = await controller.AddToCart(new AddToCartDto
    {
      ProductId = 1,
      Quantity = 2
    });

    result.Should().BeOfType<OkObjectResult>();
    (await db.Carts.CountAsync()).Should().Be(1);
    (await db.CartItems.CountAsync()).Should().Be(1);
    (await db.Products.FindAsync(1)).Quantity.Should().Be(3);
  }

  [Fact]
  public async Task UpdateQuantity_InvalidQuantity_ReturnsBadRequest()
  {
    var db = this.GetInMemoryDb();

    var product = new Product
    {
      ProductId = 1,
      Name = "Test Product",
      Description = "Test description",
      Category = "Snacks",
      Price = 10,
      Quantity = 5
    };

    var cart = new Cart
    {
      UserId = 1,
      ExpiresAt = DateTime.UtcNow.AddHours(1)
    };

    db.Products.Add(product);
    db.Carts.Add(cart);
    await db.SaveChangesAsync();

    var cartItem = new CartItem
    {
      CartId = cart.CartId,
      ProductId = 1,
      Quantity = 1,
      UnitPrice = 10
    };

    db.CartItems.Add(cartItem);
    await db.SaveChangesAsync();

    var controller = this.GetController(db);

    var result = await controller.UpdateQuantity(cartItem.CartItemId,
        new UpdateQuantityDto { Quantity = 0 });

    result.Should().BeOfType<BadRequestObjectResult>();
  }

  [Fact]
  public async Task RemoveItem_ValidItem_RemovesItemAndRestoresStock()
  {
    var db = this.GetInMemoryDb();

    var product = new Product
    {
      ProductId = 1,
      Name = "Test Product",
      Description = "Test description",
      Category = "Snacks",
      Price = 10,
      Quantity = 5
    };

    var cart = new Cart
    {
      UserId = 1,
      ExpiresAt = DateTime.UtcNow.AddHours(1)
    };

    db.Products.Add(product);
    db.Carts.Add(cart);
    await db.SaveChangesAsync();

    var cartItem = new CartItem
    {
      CartId = cart.CartId,
      ProductId = 1,
      Quantity = 2,
      UnitPrice = 10
    };

    db.CartItems.Add(cartItem);
    await db.SaveChangesAsync();

    var controller = this.GetController(db);

    var result = await controller.RemoveItem(cartItem.CartItemId);

    result.Should().BeOfType<OkObjectResult>();
    (await db.CartItems.CountAsync()).Should().Be(0);
    (await db.Products.FindAsync(1)).Quantity.Should().Be(7);
  }
}
