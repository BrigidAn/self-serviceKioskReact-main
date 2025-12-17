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

  public class OrderItemControllerTests
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

    private static OrderItemController GetController(
      AppDbContext context,
      int userId,
      bool isAdmin = false)
    {
      var controller = new OrderItemController(context);

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

    private static Product CreateValidProduct(int id, int quantity = 10)
    {
      return new Product
      {
        ProductId = id,
        Name = "Test Product",
        Category = "Beverages",
        Description = "Test description",
        Price = 10,
        Quantity = quantity
      };
    }

    [Fact]
    public async Task GetAllOrderItems_ReturnsOk_ForAdmin()
    {
      using var context = GetInMemoryDb();

      context.OrderItems.Add(new OrderItem
      {
        OrderItemId = 1,
        ProductId = 1,
        Quantity = 2,
        PriceAtPurchase = 10
      });

      await context.SaveChangesAsync();

      var controller = GetController(context, userId: 1, isAdmin: true);

      var result = await controller.GetAllOrderItems();

      result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetItemsByOrder_ReturnsOk_ForOrderOwner()
    {
      using var context = GetInMemoryDb();

      var product = CreateValidProduct(1, quantity: 50);

      var order = new Order
      {
        OrderId = 1,
        UserId = 1,
        OrderItems = new List<OrderItem>()
      };

      var item = new OrderItem
      {
        OrderItemId = 1,
        Product = product,
        Quantity = 2,
        PriceAtPurchase = 10
      };

      order.OrderItems.Add(item);

      context.Products.Add(product);
      context.Orders.Add(order);

      await context.SaveChangesAsync();

      var controller = GetController(context, userId: 1);

      var result = await controller.GetItemsByOrder(1);

      result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetItemsByOrder_ReturnsUnauthorized_ForNonOwner()
    {
      using var context = GetInMemoryDb();

      context.Orders.Add(new Order
      {
        OrderId = 1,
        UserId = 99
      });

      await context.SaveChangesAsync();

      var controller = GetController(context, userId: 1);

      var result = await controller.GetItemsByOrder(1);

      result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task AddOrderItem_AddsItem_AndReducesStock()
    {
      using var context = GetInMemoryDb();

      var product = CreateValidProduct(1, quantity: 10);

      var order = new Order
      {
        OrderId = 1,
        UserId = 1,
        OrderItems = new List<OrderItem>()
      };

      context.Products.Add(product);
      context.Orders.Add(order);
      await context.SaveChangesAsync();

      var controller = GetController(context, userId: 1);

      var dto = new CreateOrderItemDto
      {
        ProductId = 1,
        Quantity = 3
      };

      var result = await controller.AddOrderItem(1, dto);

      result.Should().BeOfType<OkObjectResult>();
      context.OrderItems.Should().HaveCount(1);
      context.Products.First().Quantity.Should().Be(7);
    }


    [Fact]
    public async Task AddOrderItem_ReturnsBadRequest_WhenInsufficientStock()
    {
      using var context = GetInMemoryDb();

      var product = CreateValidProduct(1, quantity: 1);

      context.Products.Add(product);
      context.Orders.Add(new Order { OrderId = 1, UserId = 1 });
      await context.SaveChangesAsync();

      var controller = GetController(context, userId: 1);

      var dto = new CreateOrderItemDto
      {
        ProductId = 1,
        Quantity = 5
      };

      var result = await controller.AddOrderItem(1, dto);

      result.Should().BeOfType<BadRequestObjectResult>();
    }

    // ---------------- DELETE (ADMIN) ----------------
    [Fact]
    public async Task DeleteOrderItem_RemovesItem_AndRestoresStock()
    {
      using var context = GetInMemoryDb();

      var product = CreateValidProduct(1, quantity: 5);

      var order = new Order
      {
        OrderId = 1,
        UserId = 1,
        OrderItems = new List<OrderItem>()
      };

      var item = new OrderItem
      {
        OrderItemId = 1,
        Product = product,
        Quantity = 2,
        PriceAtPurchase = 10
      };

      order.OrderItems.Add(item);

      context.Products.Add(product);
      context.Orders.Add(order);
      context.OrderItems.Add(item);
      await context.SaveChangesAsync();

      var controller = GetController(context, userId: 99, isAdmin: true);

      var result = await controller.DeleteOrderItem(1);

      result.Should().BeOfType<OkObjectResult>();
      context.OrderItems.Should().BeEmpty();
      context.Products.First().Quantity.Should().Be(7);
    }

  }
}
