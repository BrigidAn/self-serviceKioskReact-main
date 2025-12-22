using Microsoft.EntityFrameworkCore;
using KioskAPI.Data;

public static class TestDbFactory
{
  public static AppDbContext Create(string dbName)
  {
    var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(dbName)
        .ConfigureWarnings(w =>
            w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics
                .InMemoryEventId.TransactionIgnoredWarning))
        .Options;

    return new AppDbContext(options);
  }
}
