using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KioskAPI.Controllers;
using KioskAPI.Data;
using KioskAPI.Models;
using KioskAPI.Services;
using KioskAPI.Dtos;
using Microsoft.AspNetCore.Http;

public class ProductControllerTests
{
  private AppDbContext GetDbContext()
  {
    var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;

    return new AppDbContext(options);
  }

  private IFormFile FakeFile()
  {
    var stream = new MemoryStream(new byte[10]);
    return new FormFile(stream, 0, stream.Length, "file", "test.png");
  }

  [Fact]
  public async Task GetProducts_ReturnsList()
  {
    // Arrange
    var context = this.GetDbContext();

    context.Products.Add(new Product { ProductId = 1, Name = "Test", Category = "Food", Price = 10 });
    await context.SaveChangesAsync();

    var mockCloudinary = new Mock<CloudinaryService>();
    var controller = new ProductController(context, mockCloudinary.Object);

    // Act
    var result = await controller.GetProducts() as OkObjectResult;

    // Assert
    result.Should().NotBeNull();
    var list = result.Value as IEnumerable<ProductDto>;
    list.Should().HaveCount(1);
  }

  [Fact]
  public async Task GetProduct_ReturnsNotFound_WhenMissing()
  {
    var context = this.GetDbContext();
    var mockCloudinary = new Mock<CloudinaryService>();
    var controller = new ProductController(context, mockCloudinary.Object);

    var result = await controller.GetProduct(99);

    result.Should().BeOfType<NotFoundObjectResult>();
  }

  [Fact]
  public async Task AddProduct_AddsSuccessfully()
  {
    // Arrange
    var context = this.GetDbContext();
    var mockCloudinary = new Mock<CloudinaryService>();

    mockCloudinary.Setup(c => c.UploadImageAsync(It.IsAny<IFormFile>()))
                  .ReturnsAsync("http://img.com/example.png");

    var controller = new ProductController(context, mockCloudinary.Object);

    var dto = new CreateProductDto
    {
      Name = "Burger",
      Category = "Food",
      Description = "Tasty",
      Price = 20,
      Quantity = 5,
      File = this.FakeFile()
    };

    // Act
    var result = await controller.AddProduct(dto) as OkObjectResult;

    // Assert
    result.Should().NotBeNull();
    context.Products.Count().Should().Be(1);
  }

  [Fact]
  public async Task UpdateProduct_ReturnsNotFound_WhenMissing()
  {
    var context = this.GetDbContext();
    var mockCloudinary = new Mock<CloudinaryService>();

    var controller = new ProductController(context, mockCloudinary.Object);

    var dto = new UpdateProductDto { Name = "Updated" };

    var result = await controller.UpdateProduct(10, dto);

    result.Should().BeOfType<NotFoundObjectResult>();
  }

  [Fact]
  public async Task DeleteProduct_DeletesSuccessfully()
  {
    var context = this.GetDbContext();

    context.Products.Add(new Product { ProductId = 1, Name = "Test" });
    await context.SaveChangesAsync();

    var mockCloudinary = new Mock<CloudinaryService>();
    var controller = new ProductController(context, mockCloudinary.Object);

    var result = await controller.DeleteProduct(1) as OkObjectResult;

    result.Should().NotBeNull();
    context.Products.Count().Should().Be(0);
  }
}
