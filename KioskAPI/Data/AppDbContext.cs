namespace KioskAPI.Data
{
  using KioskAPI.Models;
  using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
  using Microsoft.EntityFrameworkCore;


  /// <summary>
  /// Represents the application's database context.
  /// Inherits from <see cref="IdentityDbContext{User, Role, int}"/>
  /// to provide Identity support for user authentication and roles.
  /// </summary>
  public class AppDbContext : IdentityDbContext<User, Role, int>
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="AppDbContext"/> class.
    /// </summary>
    /// <param name="options">Database context options</param>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Account> Accounts { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Checkout> Checkouts { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
      base.OnModelCreating(builder);

      List<Role> roles = new List<Role>()
      {

            new Role { Id = 1,
            Name = "Admin",
            NormalizedName = "ADMIN" },

            new Role
            { Id = 2,
            Name = "User",
            NormalizedName = "USER" }
      };
      builder.Entity<Role>().HasData(roles);
    }
  }
}