using ArtbookStore.Web.Data;
using ArtbookStore.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArtbookStore.Web.Controllers;

// This controller requires the user to be logged in for all actions by default.
[Authorize]
public class OrdersController : Controller
{
    // Database context, used to read and write data in the database.
    private readonly ApplicationDbContext _context;

    // UserManager is part of ASP.NET Core Identity and it to get information about the currently logged in user.
    private readonly UserManager<ApplicationUser> _userManager;

    // The constructor takes the database context and user manager as parameters, which are automatically provided (injected) when the controller is created.
    public OrdersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // Create, order item (add to cart). This action adds a product to the user's "Pending" order (cart).
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Customer")] // Only if the user has logged in as the "Customer" role, they can add items to the cart
    public async Task<IActionResult> Create(int productId, int quantity)
    {
        // If the user tries to add a product with zero or negative quantity, we show an error message and redirect back to the product details page.
        if (quantity <= 0)
        {
            TempData["Error"] = "Quantity must be greater than zero.";
            return RedirectToAction("Details", "Products", new { id = productId });
        }

        // First, we try to find the product in the database using the provided productId.
        var product = await _context.Products.FindAsync(productId);

        // If the product doesn't exist, shows an error message and redirect back to the product listing page.
        if (product == null)
        {
            TempData["Error"] = "Product not found.";
            return RedirectToAction("Index", "Products");
        }

        // Get the currently logged in user object to make sure we know which user is adding the product to the cart.
        var user = await _userManager.GetUserAsync(User);

        // If the user is not found (which shouldn't happen since we require authorization), we show an error and redirect back to the product listing.
        if (user == null)
        {
            TempData["Error"] = "You must be logged in.";
            return RedirectToAction("Index", "Products");
        }

        // A "Pending" order is treated as the shopping cart.
        // Check if the user already has a "Pending" order (cart).
        var order = await _context
            .Orders.Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.UserId == user.Id && o.Status == "Pending");

        // If the user has no Pending order, we create a new one. This allows users to have only one active cart at a time.
        if (order == null)
        {
            order = new Order
            {
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                Status = "Pending",
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync(); // Save so the order gets an Id
        }

        // Check if the product is already in the cart.
        var existingItem = order.OrderItems.FirstOrDefault(oi => oi.ProductId == product.Id);

        // If it exists, increase quantity.
        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
        }
        else
        {
            // If not, add a new item to the cart with the specified quantity and unit price.
            _context.OrderItems.Add(
                new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = product.Id,
                    Quantity = quantity,
                    UnitPrice = product.Price, // Store price at time of adding to cart
                }
            );
        }

        await _context.SaveChangesAsync(); // Save item change first
        await RecalculateTotal(order.Id); // Then calculate the total order price (sum of items)
        await _context.SaveChangesAsync(); // Save again because RecalculateTotal updates order.TotalAmount

        TempData["Success"] = "Product added to cart.";
        return RedirectToAction(nameof(Cart));
    }

    // Cart view with all items
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> Cart()
    {
        // Get current user's id (string)
        var userId = _userManager.GetUserId(User);

        // Load the user's Pending order, items and product info
        var order = await _context
            .Orders.Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.UserId == userId && o.Status == "Pending");

        // The view can handle order being null (empty cart).
        return View(order);
    }

    // Update quantity of an item in the cart.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> UpdateQuantity(int orderItemId, int quantity)
    {
        // Quantity must be at least 1.
        if (quantity <= 0)
        {
            TempData["Error"] = "Quantity must be at least 1.";
            return RedirectToAction(nameof(Cart));
        }

        var userId = _userManager.GetUserId(User);

        // Load item and its parent order (needed to verify ownership).
        var item = await _context
            .OrderItems.Include(oi => oi.Order)
            .FirstOrDefaultAsync(oi => oi.Id == orderItemId);

        // This check ensures that the item exists and that it is associated with an order. If either the item or the order is null, it means something went wrong (like the item was deleted), and we show an error message and redirect back to the cart page.
        if (item == null || item.Order == null)
        {
            TempData["Error"] = "Cart item not found.";
            return RedirectToAction(nameof(Cart));
        }

        // Customer can only modify their own cart items.
        if (item.Order.UserId != userId)
            return Forbid();

        // Update the quantity.
        item.Quantity = quantity;

        // Save changes then recalculate totals.
        await _context.SaveChangesAsync();
        await RecalculateTotal(item.OrderId);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Cart updated.";
        return RedirectToAction(nameof(Cart));
    }

    // Remove an item from the cart
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> RemoveItem(int orderItemId)
    {
        var userId = _userManager.GetUserId(User);

        // Load item and its order so we can check ownership.
        var item = await _context
            .OrderItems.Include(oi => oi.Order)
            .FirstOrDefaultAsync(oi => oi.Id == orderItemId);

        if (item == null || item.Order == null)
        {
            TempData["Error"] = "Item not found.";
            return RedirectToAction(nameof(Cart));
        }

        // Only allow customers to remove items from their own cart. This is a security check to prevent users from tampering with other users' carts by guessing item IDs.
        if (item.Order.UserId != userId)
            return Forbid();

        // Remove the item from the database
        _context.OrderItems.Remove(item);

        // Save then recalculate totals
        await _context.SaveChangesAsync();
        await RecalculateTotal(item.OrderId);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Item removed from cart.";
        return RedirectToAction(nameof(Cart));
    }

    // Checkout page (summary before placing order).
    [Authorize(Roles = "Customer")]
    [HttpGet]
    public async Task<IActionResult> Checkout()
    {
        var userId = _userManager.GetUserId(User);

        // Load the pending order and product info.
        var order = await _context
            .Orders.Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.UserId == userId && o.Status == "Pending");

        // If no order exists or it has no items, checkout is not possible
        if (order == null || !order.OrderItems.Any())
        {
            TempData["Error"] = "Your cart is empty.";
            return RedirectToAction(nameof(Cart));
        }

        // Make sure totals are correct before displaying checkout summary
        await RecalculateTotal(order.Id);
        await _context.SaveChangesAsync();

        return View(order);
    }

    // Place order (finalize checkout)
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> PlaceOrder()
    {
        var userId = _userManager.GetUserId(User);

        var order = await _context
            .Orders.Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.UserId == userId && o.Status == "Pending");

        if (order == null || !order.OrderItems.Any())
        {
            TempData["Error"] = "Your cart is empty.";
            return RedirectToAction(nameof(Cart));
        }

        // Check stock for each item. This is before modifying stock, to avoid partial updates.
        foreach (var item in order.OrderItems)
        {
            if (item.Product == null)
            {
                TempData["Error"] = "Product not found.";
                return RedirectToAction(nameof(Cart));
            }

            if (item.Product.StockQuantity < item.Quantity)
            {
                TempData["Error"] =
                    $"Not enough stock for {item.Product.Title}. Available: {item.Product.StockQuantity}.";
                return RedirectToAction(nameof(Cart));
            }
        }

        // Reduce stock that everything is available, so it can safely update stock.
        foreach (var item in order.OrderItems)
        {
            item.Product!.StockQuantity -= item.Quantity;
        }

        // Recalculate total and update order state.
        await RecalculateTotal(order.Id);

        // Move order out of "cart mode" into real processing state and update timestamp.
        order.Status = "Processing";
        order.CreatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Order placed successfully.";

        // Redirect to details page for the placed order
        return RedirectToAction(nameof(Details), new { id = order.Id });
    }

    // Admin index view of all active (non-archived) orders
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Index()
    {
        // Show all orders except Archived, including user info
        var orders = await _context
            .Orders.Include(o => o.User)
            .Where(o => o.Status != "Archived")
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        // Uses a dedicated admin view name
        return View("AdminIndex", orders);
    }

    // Archived orders view
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Archived()
    {
        var orders = await _context
            .Orders.Include(o => o.User)
            .Where(o => o.Status == "Archived")
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return View("ArchivedIndex", orders);
    }

    // Order details view for both customers and admins
    [Authorize(Roles = "Customer,Admin")]
    public async Task<IActionResult> Details(int id)
    {
        var userId = _userManager.GetUserId(User);

        // Load the order, its items, products, and user
        var order = await _context
            .Orders.Include(o => o.User)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        // If the order doesn't exist, show an error and redirect back to home page. This prevents users from seeing a blank page or an error if they try to access an order that doesn't exist.
        if (order == null)
        {
            TempData["Error"] = "Order not found.";
            return RedirectToAction("Index", "Home");
        }

        // If customer, they can only view their own order.
        if (User.IsInRole("Customer") && order.UserId != userId)
            return Forbid();

        return View(order);
    }

    // Admin update order status
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        var order = await _context.Orders.FindAsync(id);

        if (order == null)
        {
            TempData["Error"] = "Order not found.";
            return RedirectToAction(nameof(Index));
        }

        // Allowed status values, to prevent invalid values being saved.
        var allowedStatuses = new[]
        {
            "Pending",
            "Processing",
            "Completed",
            "Cancelled",
            "Archived",
        };

        // If the status is not in our allowed list, reject it.
        if (!allowedStatuses.Contains(status))
        {
            TempData["Error"] = "Invalid status value.";
            return RedirectToAction(nameof(Index));
        }

        // Update status and save
        order.Status = status;
        await _context.SaveChangesAsync();

        TempData["Success"] = $"Order #{order.Id} status updated to {status}.";
        return RedirectToAction(nameof(Index));
    }

    // Customer's order history (excluding pending orders)
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> MyOrders()
    {
        var userId = _userManager.GetUserId(User);

        // Show all orders except Pending (Pending is the cart)
        var orders = await _context
            .Orders.Where(o => o.UserId == userId && o.Status != "Pending")
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return View(orders);
    }

    // Helper method to recalculate total amount of an order
    // Calculates the total amount of an order by summing: Quantity x UnitPrice for each order item.
    private async Task RecalculateTotal(int orderId)
    {
        // SumAsync returns null if there are no rows, so we use (decimal?) and ?? 0
        var total =
            await _context
                .OrderItems.Where(oi => oi.OrderId == orderId)
                .SumAsync(oi => (decimal?)oi.Quantity * oi.UnitPrice) ?? 0;

        // Find the order and update TotalAmount field. This is a separate query because we need to update the order record after calculating the total.
        var order = await _context.Orders.FindAsync(orderId);

        if (order != null)
        {
            order.TotalAmount = total;
        }
    }
}
