using ArtbookStore.Web.Models;

namespace ArtbookStore.Web.Models.ViewModels;

// ViewModel for the home page, containing featured products and categories.
public class HomeViewModel
{
    public List<Product> FeaturedProducts { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
}
