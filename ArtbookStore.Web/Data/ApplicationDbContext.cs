using ArtbookStore.Web.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ArtbookStore.Web.Data;

// ApplicationDbContext inherits from IdentityDbContext<ApplicationUser> to include ASP.NET Identity tables (AspNetUsers, AspNetRoles, etc.)
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    // The constructor receives database configuration (connection string, provider, etc.) and passes it to the base class.
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    // DbSets for our domain models that each DbSet represents a table in the database.
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    // Model configuration are the method allows to configure how models behave in the database.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Call the base method to ensure Identity models are configured
        base.OnModelCreating(modelBuilder);

        // Define table names (optional, EF Core will pluralize by default) to prevent SQL truncation issues for monetary values
        modelBuilder.Entity<Product>().Property(p => p.Price).HasPrecision(18, 2);

        modelBuilder.Entity<Order>().Property(o => o.TotalAmount).HasPrecision(18, 2);

        modelBuilder.Entity<OrderItem>().Property(oi => oi.UnitPrice).HasPrecision(18, 2);

        // Relations configuration
        // Order to OrderItems (1-to-many). If an Order is deleted, its OrderItems will also be deleted (Cascade).
        modelBuilder
            .Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.OrderItems)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Product to OrderItems (many-to-one). Many OrderItems can reference the same Product. If a Product is deleted, we want to prevent deletion if there are OrderItems referencing it (Restrict) to maintain data integrity.
        modelBuilder
            .Entity<OrderItem>()
            .HasOne(oi => oi.Product)
            .WithMany()
            .HasForeignKey(oi => oi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Order to ApplicationUser (many-to-one). A User can have many Orders. A User cannot be deleted if they have orders.
        modelBuilder
            .Entity<Order>()
            .HasOne(o => o.User)
            .WithMany()
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
