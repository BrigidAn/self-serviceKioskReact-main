namespace KioskAPI.Tests.Fixtures
{
  using System;
  using AutoMapper;
  using Microsoft.EntityFrameworkCore;
  using KioskAPI.Data;
  using KioskAPI.Mappers;

  public static class TestContextFactory
  {
    public static AppDbContext CreateContext(string? name = null)
    {
      var options = new DbContextOptionsBuilder<AppDbContext>()
          .UseInMemoryDatabase(name ?? Guid.NewGuid().ToString())
          .Options;

      var ctx = new AppDbContext(options);
      ctx.Database.EnsureDeleted();
      ctx.Database.EnsureCreated();
      return ctx;
    }

    public static IMapper CreateMapper()
    {
      var config = new MapperConfiguration(cfg =>
      {
        cfg.AddProfile(new MappingProfile());
      });

      config.AssertConfigurationIsValid();
      return config.CreateMapper();
    }
  }
}
