namespace KioskAPI
{
  using Microsoft.EntityFrameworkCore;
  using Microsoft.EntityFrameworkCore.Design;
  using KioskAPI.Data;

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
  }
}
