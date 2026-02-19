using System.ComponentModel.DataAnnotations;

namespace ArtbookStore.Web.Models.ViewModels;

// ViewModel for editing an existing product.
public class ProductEditViewModel
{
    public int Id { get; set; }

    [Required]
    public string? Title { get; set; }

    [Required]
    public string? Author { get; set; }

    public string? Description { get; set; }

    [Required]
    public decimal Price { get; set; }

    public string? ImageUrl { get; set; }

    public int StockQuantity { get; set; }

    [Required]
    public int CategoryId { get; set; }
}
