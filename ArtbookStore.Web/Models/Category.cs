using System.ComponentModel.DataAnnotations;

namespace ArtbookStore.Web.Models;

// Create a Category class with CategoryId and Name properties
public class Category
{
    // Primary key for Category
    public int CategoryId { get; set; }

    // Name of the category that is required and has a maximum length of 100 characters
    [Required]
    [MaxLength(100)]
    public string? Name { get; set; }

    // Slug of the category
    [MaxLength(120)]
    public string? Slug { get; set; }

    // Added an optional ImageUrl property to store the URL of the category image in home page
    public string? ImageUrl { get; set; }

    // Navigation property for one category can have many products
    public ICollection<Product>? Products { get; set; }
}
