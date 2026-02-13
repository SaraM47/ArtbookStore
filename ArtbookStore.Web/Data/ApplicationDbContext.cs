using ArtbookStore.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace ArtbookStore.Web.Data;

// Added ApplicationDbContext class that inherits from DbContext and has DbSet properties for Categories and Products
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
}
