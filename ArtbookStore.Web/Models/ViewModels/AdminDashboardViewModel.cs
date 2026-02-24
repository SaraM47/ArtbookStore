using ArtbookStore.Web.Models;

// Create a view model to hold the data for the admin dashboard
public class AdminDashboardViewModel
{
    // Overview cards
    public int TotalProducts { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public int LowStockCount { get; set; }

    // Latest activity
    public List<Order> RecentOrders { get; set; } = new();
    public List<Product> LowStockProducts { get; set; } = new();

    // Revenue chart data (monthly, Completed orders only)
    public List<string> RevenueLabels { get; set; } = new();
    public List<decimal> RevenueData { get; set; } = new();
}
