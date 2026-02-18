using System.ComponentModel.DataAnnotations;

namespace ArtbookStore.Web.Models;

// Create a Category class with CategoryId and Name properties
public class Category
{
    // Primary key for Category
    public int CategoryId { get; set; }

    // Name of the category that is required
    [Required]
    public string? Name { get; set; }

    // Navigation property for one category can have many products
    public ICollection<Product>? Products { get; set; }
}
