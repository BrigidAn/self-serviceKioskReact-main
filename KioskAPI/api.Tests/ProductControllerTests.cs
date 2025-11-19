using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using KioskAPI.Controllers;
using KioskAPI.Data;
using KioskAPI.Dtos;
using KioskAPI.Models;
using KioskAPI.Tests.Fixtures;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace KioskAPI.api.Tests
{
    public class ProductControllerTests
    {
       private AppDbContext CreateSeeded(out IMapper mapper)
        {
            var ctx = TestContextFactory.CreateContext();
            mapper = TestContextFactory.CreateMapper();

            // Seed suppliers
            var supplier1 = new Supplier { SupplierId = 1, Name = "Acme Supplies" };
            var supplier2 = new Supplier { SupplierId = 2, Name = "BestGoods" };

            ctx.Suppliers.AddRange(supplier1, supplier2);

            // Seed products mock data
            ctx.Products.AddRange(
                new Product { ProductId = 1, Name = "Apple", Description = "Fresh Apple", Category = "Fruit", Price = 1.25m, Quantity = 10, SupplierId = 1 },
                new Product { ProductId = 2, Name = "Banana", Description = "Yellow Banana", Category = "Fruit", Price = 0.75m, Quantity = 30, SupplierId = 1 },
                new Product { ProductId = 3, Name = "Shampoo", Description = "Hair care", Category = "Personal Care", Price = 5.99m, Quantity = 5, SupplierId = 2 }
            );

            ctx.SaveChanges();
            return ctx;
        }

        [Fact] // returns all products from seeded data
        public async Task GetProducts_ReturnsAllProducts()
        {
            var ctx = CreateSeeded(out var mapper);
            var controller = new ProductController(ctx, mapper);

            var result = await controller.GetProducts();

            result.Should().BeOfType<OkObjectResult>();
            var products = (result as OkObjectResult).Value as List<ProductDto>;
            products.Should().NotBeNull();
            products.Count.Should().Be(3);
            products.Any(p => p.Name == "Apple").Should().BeTrue();
        }

        [Fact] // returns specifc product by their id
        public async Task GetProduct_ExistingId_ReturnsProduct()
        {
            var ctx = CreateSeeded(out var mapper);
            var controller = new ProductController(ctx, mapper);

            var result = await controller.GetProduct(1);

            result.Should().BeOfType<OkObjectResult>();
            var product = (result as OkObjectResult).Value as ProductDto;
            product.Should().NotBeNull();
            product.Name.Should().Be("Apple");
        }

        [Fact] // returns products that do not exist
        public async Task GetProduct_NonExistingId_ReturnsNotFound()
        {
            var ctx = CreateSeeded(out var mapper);
            var controller = new ProductController(ctx, mapper);

            var result = await controller.GetProduct(999);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact] // create/adds a new product
        public async Task AddProduct_NegativeQuantity_ReturnsBadRequest()
        {
            var ctx = CreateSeeded(out var mapper);
            var controller = new ProductController(ctx, mapper);

            var dto = new CreateProductDto
            {
                Name = "Faulty Item",
                Quantity = -5,
                Price = 1m,
                Category = "Misc",
                SupplierId = 1
            };

            var result = await controller.AddProduct(dto);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task UpdateProduct_ExistingId_UpdatesProduct() //update exsiting products
        {
            var ctx = CreateSeeded(out var mapper);
            var controller = new ProductController(ctx, mapper);

            var dto = new UpdateProductDto
            {
                Name = "Apple Updated",
                Price = 1.50m,
                Quantity = 15
            };

            var result = await controller.UpdateProduct(1, dto);

            result.Should().BeOfType<OkObjectResult>();
            var updated = await ctx.Products.FindAsync(1);
            updated.Name.Should().Be("Apple Updated");
            updated.Price.Should().Be(1.50m);
            updated.Quantity.Should().Be(15);
        }

        [Fact]
        public async Task UpdateProduct_NonExistingId_ReturnsNotFound()
        {
            var ctx = CreateSeeded(out var mapper);
            var controller = new ProductController(ctx, mapper);

            var dto = new UpdateProductDto { Name = "DoesNotExist" };
            var result = await controller.UpdateProduct(999, dto);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task DeleteProduct_ExistingId_RemovesProduct()
        {
            var ctx = CreateSeeded(out var mapper);
            var controller = new ProductController(ctx, mapper);

            var result = await controller.DeleteProduct(1);

            result.Should().BeOfType<OkObjectResult>();
            ctx.Products.Count().Should().Be(2);
            ctx.Products.Any(p => p.ProductId == 1).Should().BeFalse();
        }

        [Fact]
        public async Task DeleteProduct_NonExistingId_ReturnsNotFound()
        {
            var ctx = CreateSeeded(out var mapper);
            var controller = new ProductController(ctx, mapper);

            var result = await controller.DeleteProduct(999);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        // [Fact]
        // public async Task FilterProducts_ByCategory_ReturnsFilteredResults()
        // {
        //     var ctx = CreateSeeded(out var mapper);
        //     var controller = new ProductController(ctx, mapper);

        //     var filter = new ProductFilterDto { Category = "Fruit" };
        //     var result = await controller.FilterProducts(filter);

        //     result.Should().BeOfType<OkObjectResult>();
        //     var data = (result as OkObjectResult).Value as dynamic;
        //     ((List<ProductDto>)data.products).All(p => p.Category == "Fruit").Should().BeTrue();
        // }


    }
}