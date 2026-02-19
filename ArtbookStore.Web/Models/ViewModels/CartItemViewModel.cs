namespace ArtbookStore.Web.Models.ViewModels;

// ViewModel for items in the shopping cart.
public class CartItemViewModel
{
    public int ProductId { get; set; }

    public string Title { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal LineTotal => Quantity * UnitPrice;
}
