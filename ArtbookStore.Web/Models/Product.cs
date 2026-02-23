using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtbookStore.Web.Models;

// Create a Product class. This includes with ProductId, Name, Description, Price, ImageUrl, and CategoryId properties
public class Product
{
    public int Id { get; set; } // Primary key

    [Required]
    public string? Title { get; set; }

    [Required]
    public string? Author { get; set; }

    public string? Description { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    public string? ImageUrl { get; set; }

    public int StockQuantity { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Foreign key
    public int CategoryId { get; set; }

    // Navigation property
    public Category? Category { get; set; }
}
