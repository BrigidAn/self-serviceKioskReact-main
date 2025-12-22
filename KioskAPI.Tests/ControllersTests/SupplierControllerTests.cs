namespace KioskAPI.Tests.ControllersTests
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
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

  public class SupplierControllerTests
  {
    private AppDbContext GetInMemoryDb()
    {
      var options = new DbContextOptionsBuilder<AppDbContext>()
          .UseInMemoryDatabase(Guid.NewGuid().ToString())
          .Options;
      return new AppDbContext(options);
    }

    private SupplierController GetController(AppDbContext context)
    {
      var controller = new SupplierController(context);
      controller.ControllerContext = new ControllerContext
      {
        HttpContext = new DefaultHttpContext()
      };
      return controller;
    }

    [Fact]
    public async Task GetAllSuppliers_ReturnsOk_WithSuppliers()
    {
      using var context = this.GetInMemoryDb();
      context.Suppliers.AddRange(
          new Supplier { SupplierId = 1, Name = "Supplier A", ContactInfo = "A Contact" },
          new Supplier { SupplierId = 2, Name = "Supplier B", ContactInfo = "B Contact" }
      );
      await context.SaveChangesAsync();

      var controller = this.GetController(context);

      var result = await controller.GetAllSuppliers();
      var okResult = result as OkObjectResult;
      okResult.Should().NotBeNull();

      var suppliers = okResult.Value as List<SupplierDto>;
      suppliers.Should().NotBeNull();
      suppliers.Count.Should().Be(2);
    }

    [Fact]
    public async Task GetSupplierById_ReturnsOk_WhenSupplierExists()
    {
      using var context = this.GetInMemoryDb();
      context.Suppliers.Add(new Supplier { SupplierId = 1, Name = "Supplier A", ContactInfo = "Contact A" });
      await context.SaveChangesAsync();

      var controller = this.GetController(context);

      var result = await controller.GetSupplierById(1);
      var okResult = result as OkObjectResult;
      okResult.Should().NotBeNull();

      var supplier = okResult.Value as SupplierDto;
      supplier.Should().NotBeNull();
      supplier.SupplierId.Should().Be(1);
      supplier.Name.Should().Be("Supplier A");
    }

    [Fact]
    public async Task GetSupplierById_ReturnsNotFound_WhenSupplierDoesNotExist()
    {
      using var context = this.GetInMemoryDb();
      var controller = this.GetController(context);

      var result = await controller.GetSupplierById(99);
      result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task AddSupplier_ReturnsOk_WhenValidDto()
    {
      using var context = this.GetInMemoryDb();

      var controller = new SupplierController(context);

      var dto = new SupplierCreateDto
      {
        Name = "Test Supplier",
        ContactInfo = "test contact"
      };

      var result = await controller.AddSupplier(dto);

      var ok = result.Should().BeOfType<OkObjectResult>().Subject;

      var message = ok.Value
          .GetType()
          .GetProperty("message")!
          .GetValue(ok.Value) as string;

      message.Should().Be("Supplier added successfully.");
    }

    [Fact]
    public async Task AddSupplier_ReturnsBadRequest_WhenNameIsMissing()
    {
      using var context = this.GetInMemoryDb();
      var controller = this.GetController(context);

      var dto = new SupplierCreateDto { Name = "", ContactInfo = "Contact" };
      var result = await controller.AddSupplier(dto);

      result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UpdateSupplier_ReturnsOk_WhenSupplierExists()
    {
      using var context = this.GetInMemoryDb();
      context.Suppliers.Add(new Supplier { SupplierId = 1, Name = "Old Name", ContactInfo = "Old Contact" });
      await context.SaveChangesAsync();

      var controller = this.GetController(context);
      var dto = new SupplierUpdateDto { Name = "Updated Name", ContactInfo = "Updated Contact" };

      var result = await controller.UpdateSupplier(1, dto);
      var okResult = result as OkObjectResult;
      okResult.Should().NotBeNull();

      var supplier = await context.Suppliers.FindAsync(1);
      supplier.Name.Should().Be("Updated Name");
      supplier.ContactInfo.Should().Be("Updated Contact");
    }

    [Fact]
    public async Task UpdateSupplier_ReturnsNotFound_WhenSupplierDoesNotExist()
    {
      using var context = this.GetInMemoryDb();
      var controller = this.GetController(context);
      var dto = new SupplierUpdateDto { Name = "Updated", ContactInfo = "Contact" };

      var result = await controller.UpdateSupplier(99, dto);
      result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DeleteSupplier_ReturnsOk_WhenSupplierHasNoProducts()
    {
      using var context = this.GetInMemoryDb();
      context.Suppliers.Add(new Supplier { SupplierId = 1, Name = "Supplier A", ContactInfo = "Contact" });
      await context.SaveChangesAsync();

      var controller = this.GetController(context);
      var result = await controller.DeleteSupplier(1);

      var okResult = result as OkObjectResult;
      okResult.Should().NotBeNull();
      ((string)okResult.Value.GetType().GetProperty("message").GetValue(okResult.Value)).Should().Be("Supplier deleted successfully.");
    }

    [Fact]
    public async Task DeleteSupplier_ReturnsBadRequest_WhenSupplierHasProducts()
    {
      using var context = this.GetInMemoryDb();
      context.Suppliers.Add(new Supplier { SupplierId = 1, Name = "Supplier A", ContactInfo = "Contact" });
      context.Products.Add(new Product { ProductId = 1, Name = "Prod", SupplierId = 1, Category = "Cat", Description = "Desc", Price = 50, Quantity = 5 });
      await context.SaveChangesAsync();

      var controller = this.GetController(context);
      var result = await controller.DeleteSupplier(1);

      var badRequest = result as BadRequestObjectResult;
      badRequest.Should().NotBeNull();
      ((string)badRequest.Value.GetType().GetProperty("message").GetValue(badRequest.Value))
          .Should().Be("Cannot delete supplier with products assigned.");
    }

    [Fact]
    public async Task DeleteSupplier_ReturnsNotFound_WhenSupplierDoesNotExist()
    {
      using var context = this.GetInMemoryDb();
      var controller = this.GetController(context);

      var result = await controller.DeleteSupplier(99);
      result.Should().BeOfType<NotFoundObjectResult>();
    }
  }
}
