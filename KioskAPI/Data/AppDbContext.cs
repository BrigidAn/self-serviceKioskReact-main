namespace KioskAPI.Data
{
  using KioskAPI.Models;
  using Microsoft.EntityFrameworkCore;

  public class AppDbContext : DbContext
  {
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // DbSets map to tables
    public DbSet<Role> Roles => this.Set<Role>();//public DbSet<Roel> Roles {get; set;}
    public DbSet<User> Users => this.Set<User>();
    public DbSet<Account> Accounts => this.Set<Account>();
    public DbSet<Transaction> Transactions => this.Set<Transaction>();
    public DbSet<Order> Orders => this.Set<Order>();
    public DbSet<OrderItem> OrderItems => this.Set<OrderItem>();
    public DbSet<Product> Products => this.Set<Product>();
    public DbSet<Supplier> Suppliers => this.Set<Supplier>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      base.OnModelCreating(modelBuilder);

      // Seed roles and an initial admin user for convenience
      modelBuilder.Entity<Role>().HasData(
          new Role { RoleId = 1, RoleName = "Admin", Description = "Full access" },
          new Role { RoleId = 2, RoleName = "User", Description = "Regular user" }
      );
    }
  }
}