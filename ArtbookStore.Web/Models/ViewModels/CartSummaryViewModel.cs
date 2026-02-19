namespace ArtbookStore.Web.Models.ViewModels;

// ViewModel for displaying a summary of the shopping cart.
public class CartSummaryViewModel
{
    public int ItemCount { get; set; }
    public decimal TotalAmount { get; set; }
    public List<CartItemDto> Items { get; set; } = new();
}

// This is a simple DTO (Data Transfer Object) for cart items, used in the CartSummaryViewModel.
public class CartItemDto
{
    public string? Title { get; set; }
    public int Quantity { get; set; }
}
