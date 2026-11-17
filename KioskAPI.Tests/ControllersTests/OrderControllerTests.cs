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
  using Microsoft.Extensions.Logging;
  using Moq;

  public class OrderControllerTests
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

    private static OrderController GetController(
     AppDbContext context,
     int userId,
     bool isAdmin = false)
    {
      var logger = new Mock<ILogger<OrderController>>();

      var controller = new OrderController(context, logger.Object);

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
    public async Task GetAllOrders_ReturnsOk_ForAdmin()
    {
      using var context = GetInMemoryDb();

      context.Orders.Add(new Order
      {
        UserId = 1,
        Status = "Pending",
        PaymentStatus = "Unpaid",
        TotalAmount = 100
      });

      await context.SaveChangesAsync();

      var controller = GetController(context, userId: 99, isAdmin: true);

      var result = await controller.GetAllOrders();

      result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetMyOrders_ReturnsOnlyUserOrders()
    {
      using var context = GetInMemoryDb();

      context.Orders.AddRange(
        new Order { UserId = 1, TotalAmount = 100 },
        new Order { UserId = 2, TotalAmount = 200 }
      );

      await context.SaveChangesAsync();

      var controller = GetController(context, userId: 1);

      var result = await controller.GetMyOrders();

      var ok = result as OkObjectResult;
      ok.Should().NotBeNull();

      var orders = ok!.Value as IEnumerable<object>;
      orders!.Count().Should().Be(1);
    }

    [Fact]
    public async Task CreateOrder_CreatesOrder_WhenStockIsAvailable()
    {
      using var context = GetInMemoryDb();

      context.Products.Add(new Product
      {
        ProductId = 1,
        Name = "Test Product",
        Price = 50,
        Quantity = 10,
        Category = "Food",
        Description = "Test"
      });

      await context.SaveChangesAsync();

      var controller = GetController(context, userId: 1);

      var dto = new CreateOrderDto
      {
        UserId = 1,
        Items = new List<CreateOrderItemDto>
        {
          new CreateOrderItemDto { ProductId = 1, Quantity = 2 }
        }
      };

      var result = await controller.CreateOrder(dto);

      result.Should().BeOfType<OkObjectResult>();

      context.Orders.Should().HaveCount(1);
      context.OrderItems.Should().HaveCount(1);

      var product = await context.Products.FindAsync(1);
      product!.Quantity.Should().Be(8);
    }

    [Fact]
    public async Task CreateOrder_NonAdmin_CannotCreateOrderForAnotherUser()
    {
      using var context = GetInMemoryDb();

      context.Products.Add(new Product
      {
        ProductId = 1,
        Name = "Test Product",
        Price = 50,
        Quantity = 10,
        Category = "Food",
        Description = "Test"
      });
      await context.SaveChangesAsync();

      var controller = GetController(context, userId: 1);

      var result = await controller.CreateOrder(new CreateOrderDto
      {
        UserId = 2, // tries to order on someone else's behalf
        Items = new List<CreateOrderItemDto> { new CreateOrderItemDto { ProductId = 1, Quantity = 1 } }
      });

      result.Should().BeOfType<OkObjectResult>();
      context.Orders.Should().ContainSingle().Which.UserId.Should().Be(1);
    }

    [Fact]
    public async Task CreateOrder_ReturnsBadRequest_WhenInsufficientStock()
    {
      using var context = GetInMemoryDb();

      context.Products.Add(new Product
      {
        ProductId = 1,
        Name = "Test Product",
        Price = 50,
        Quantity = 1,
        Category = "Food",
        Description = "Test"
      });

      await context.SaveChangesAsync();

      var controller = GetController(context, userId: 1);

      var dto = new CreateOrderDto
      {
        UserId = 1,
        Items = new List<CreateOrderItemDto>
        {
          new CreateOrderItemDto { ProductId = 1, Quantity = 5 }
        }
      };

      var result = await controller.CreateOrder(dto);

      result.Should().BeOfType<BadRequestObjectResult>();
      context.Orders.Should().BeEmpty();
    }

    [Fact]
    public async Task CompleteOrder_MarksOrderAsComplete()
    {
      using var context = GetInMemoryDb();

      var order = new Order
      {
        OrderId = 1,
        UserId = 1,
        Status = "Pending",
        PaymentStatus = "Unpaid",
        TotalAmount = 100m,
        OrderItems = new List<OrderItem>()
      };

      context.Orders.Add(order);
      context.Accounts.Add(new Account { AccountId = 1, UserId = 1, Balance = 250m });
      await context.SaveChangesAsync();

      var controller = GetController(context, userId: 1);

      var result = await controller.CompleteOrder(1);

      result.Should().BeOfType<OkObjectResult>();

      var updatedOrder = await context.Orders.FindAsync(1);
      updatedOrder!.Status.Should().Be("Completed");
      updatedOrder.PaymentStatus.Should().Be("Paid");

      var account = await context.Accounts.FindAsync(1);
      account!.Balance.Should().Be(150m);
      context.Transactions.Should().ContainSingle(t => t.OrderId == 1 && t.TotalAmount == 100m);
    }

    [Fact]
    public async Task CompleteOrder_ReturnsBadRequest_WhenBalanceInsufficient()
    {
      using var context = GetInMemoryDb();

      context.Orders.Add(new Order
      {
        OrderId = 1,
        UserId = 1,
        Status = "Pending",
        PaymentStatus = "Unpaid",
        TotalAmount = 100m,
        OrderItems = new List<OrderItem>()
      });
      context.Accounts.Add(new Account { AccountId = 1, UserId = 1, Balance = 40m });
      await context.SaveChangesAsync();

      var controller = GetController(context, userId: 1);

      var result = await controller.CompleteOrder(1);

      result.Should().BeOfType<BadRequestObjectResult>();
      var unchanged = await context.Orders.FindAsync(1);
      unchanged!.PaymentStatus.Should().Be("Unpaid");
      (await context.Accounts.FindAsync(1))!.Balance.Should().Be(40m);
    }

    [Fact]
    public async Task CompleteOrder_ReturnsBadRequest_WhenAlreadyPaid()
    {
      using var context = GetInMemoryDb();

      context.Orders.Add(new Order
      {
        OrderId = 1,
        UserId = 1,
        Status = "Completed",
        PaymentStatus = "Paid",
        TotalAmount = 100m,
        OrderItems = new List<OrderItem>()
      });
      context.Accounts.Add(new Account { AccountId = 1, UserId = 1, Balance = 500m });
      await context.SaveChangesAsync();

      var controller = GetController(context, userId: 1);

      var result = await controller.CompleteOrder(1);

      result.Should().BeOfType<BadRequestObjectResult>();
      (await context.Accounts.FindAsync(1))!.Balance.Should().Be(500m);
    }

    [Fact]
    public async Task CompleteOrder_ReturnsNotFound_WhenOrderDoesNotExist()
    {
      using var context = GetInMemoryDb();

      var controller = GetController(context, userId: 1);

      var result = await controller.CompleteOrder(999);

      result.Should().BeOfType<NotFoundObjectResult>();
    }
  }
}