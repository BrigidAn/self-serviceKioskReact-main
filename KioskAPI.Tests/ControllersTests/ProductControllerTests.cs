namespace KioskAPI.Tests.ControllersTests
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using KioskAPI.Controllers;
  using KioskAPI.Data;
  using KioskAPI.Dtos;
  using KioskAPI.Models;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.AspNetCore.Http;
  using System.IO;
  using Xunit;

  public class ProductControllerTests
  {
    private AppDbContext CreateContext()
    {
      return TestContextFactory.CreateContext();
    }

    private Supplier CreateSupplier()
    {
      return new Supplier
      {
        SupplierId = 1,
        Name = "Test Supplier"
      };
    }

    [Fact]
    public async Task GetProducts_Returns_All_Products()
    {
      var context = this.CreateContext();
      var supplier = this.CreateSupplier();

      context.Suppliers.Add(supplier);
      context.Products.AddRange(
          new Product
          {
            ProductId = 1,
            Name = "A",
            Category = "Cat",
            Description = "Product A description",
            Price = 10,
            Quantity = 1,
            Supplier = supplier
          },
          new Product
          {
            ProductId = 2,
            Name = "B",
            Category = "Cat",
            Description = "Product B description",
            Price = 20,
            Quantity = 2,
            Supplier = supplier
          }
      );
      await context.SaveChangesAsync();

      var controller = new ProductController(context, null);

      var result = await controller.GetProducts();

      var ok = Assert.IsType<OkObjectResult>(result);
      var products = Assert.IsAssignableFrom<List<ProductDto>>(ok.Value);
      Assert.Equal(2, products.Count);
    }

    [Fact]
    public async Task GetProduct_Returns_Product_When_Found()
    {
      var context = this.CreateContext();
      var supplier = this.CreateSupplier();

      var product = new Product
      {
        ProductId = 1,
        Name = "Test",
        Category = "Cat",
        Price = 10,
        Quantity = 1,
        Description = "Test product description",
        Supplier = supplier
      };

      context.Suppliers.Add(supplier);
      context.Products.Add(product);
      await context.SaveChangesAsync();

      var controller = new ProductController(context, null);

      var result = await controller.GetProduct(1);

      var ok = Assert.IsType<OkObjectResult>(result);
      var dto = Assert.IsType<ProductDto>(ok.Value);
      Assert.Equal("Test", dto.Name);
    }

    [Fact]
    public async Task GetProduct_Returns_NotFound_When_Missing()
    {
      var context = this.CreateContext();
      var controller = new ProductController(context, null);

      var result = await controller.GetProduct(999);

      Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task UpdateProduct_Returns_NotFound_When_Invalid_Id()
    {
      var context = this.CreateContext();
      var controller = new ProductController(context, null);

      var dto = new UpdateProductDto { Name = "Updated" };

      var result = await controller.UpdateProduct(99, dto);

      Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task DeleteProduct_Removes_Product()
    {
      var context = this.CreateContext();
      var supplier = this.CreateSupplier();

      var product = new Product
      {
        ProductId = 1,
        Name = "DeleteMe",
        Category = "Cat",
        Description = "To be deleted",
        Price = 5,
        Quantity = 1,
        Supplier = supplier
      };

      context.Suppliers.Add(supplier);
      context.Products.Add(product);
      await context.SaveChangesAsync();

      var controller = new ProductController(context, null);

      var result = await controller.DeleteProduct(1);

      Assert.IsType<OkObjectResult>(result);
      Assert.Empty(context.Products);
    }
  }
}
