using ArtbookStore.Web.Data;
using ArtbookStore.Web.Models;
using ArtbookStore.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

// This attribute means only users with the role "Admin" are allowed to access this controller.
[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    // This controller is responsible for the admin dashboard, which shows key metrics and recent activity.
    // This is our database context and use it to communicate with the database.
    private readonly ApplicationDbContext _context;

    // These are configuration settings related to inventory, for example: how many items count as "low stock".
    private readonly InventorySettings _inventorySettings;

    /*
    * The constructor takes the database context and inventory settings as parameters. This means automatically injects (provides) the database context and the inventory settings when this controller is created.
    */
    public AdminController(
        ApplicationDbContext context,
        IOptions<InventorySettings> inventorySettings
    )
    {
        // Save the injected database context so we can use it later.
        _context = context;
        // Save the inventory settings and .Value is needed. It's because IOptions wraps the actual settings object.
        _inventorySettings = inventorySettings.Value;
    }

    // This action method loads the Admin Dashboard page.
    // "async" means it runs asynchronously, which improves performance.
    public async Task<IActionResult> Index()
    {
        // Get the low stock threshold value from configuration.
        var threshold = _inventorySettings.LowStockThreshold;

        // Create a new ViewModel that will contain all dashboard data.
        var vm = new AdminDashboardViewModel
        {
            // Count the total number of products in the database.
            TotalProducts = await _context.Products.CountAsync(),

            // Count the total number of orders.
            TotalOrders = await _context.Orders.CountAsync(),

            /*
            * Calculate total revenue by summing the TotalAmount of all orders that have the status "Completed". We use a nullable decimal (decimal?) to handle cases where there might be no completed orders and prevent errors if there are no matching rows. If result is null, we default to 0 using the null-coalescing operator (??) instead.
            */
            TotalRevenue =
                await _context
                    .Orders.Where(o => o.Status == "Completed")
                    .SumAsync(o => (decimal?)o.TotalAmount) ?? 0,

            // Count how many products have stock lower than the threshold.
            LowStockCount = await _context.Products.CountAsync(p => p.StockQuantity < threshold),

            // Get the 5 most recent orders (newest first).
            RecentOrders = await _context
                .Orders.OrderByDescending(o => o.CreatedAt)
                .Take(5)
                .ToListAsync(),
            // Get all products that have low stock.
            LowStockProducts = await _context
                .Products.Where(p => p.StockQuantity < threshold)
                .ToListAsync(),
        };

        // Monthly revenue for chart (Completed orders only)
        // First, get all completed orders.
        var completedOrders = await _context
            .Orders.Where(o => o.Status == "Completed")
            .ToListAsync();

        // Group orders by Year and Month.
        // Then calculate the total revenue for each month.
        var monthlyRevenue = completedOrders
            .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
            .Select(g => new
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                Total = g.Sum(o => o.TotalAmount), // Sum of TotalAmount for that month
            })
            // Order results chronologically
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Month)
            .ToList();

        // Create labels for the chart (example: 2026-02).
        vm.RevenueLabels = monthlyRevenue.Select(x => $"{x.Year}-{x.Month:D2}").ToList();

        // Create data values for the chart (revenue numbers).
        vm.RevenueData = monthlyRevenue.Select(x => x.Total).ToList();

        // Send the ViewModel to the View (Admin Dashboard page).
        return View(vm);
    }
}
