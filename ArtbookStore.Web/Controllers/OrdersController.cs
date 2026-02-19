using ArtbookStore.Web.Data;
using ArtbookStore.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArtbookStore.Web.Controllers;

[Authorize]
public class OrdersController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public OrdersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // Create order item (add to cart)
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> Create(int productId, int quantity)
    {
        if (quantity <= 0)
        {
            TempData["Error"] = "Quantity must be greater than zero.";
            return RedirectToAction("Details", "Products", new { id = productId });
        }

        var product = await _context.Products.FindAsync(productId);

        if (product == null)
        {
            TempData["Error"] = "Product not found.";
            return RedirectToAction("Index", "Products");
        }

        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            TempData["Error"] = "You must be logged in.";
            return RedirectToAction("Index", "Products");
        }

        var order = await _context
            .Orders.Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.UserId == user.Id && o.Status == "Pending");

        if (order == null)
        {
            order = new Order
            {
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                Status = "Pending",
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
        }

        var existingItem = order.OrderItems.FirstOrDefault(oi => oi.ProductId == product.Id);

        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
        }
        else
        {
            _context.OrderItems.Add(
                new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = product.Id,
                    Quantity = quantity,
                    UnitPrice = product.Price,
                }
            );
        }

        await RecalculateTotal(order.Id);

        TempData["Success"] = "Product added to cart.";
        return RedirectToAction(nameof(Cart));
    }

    // Cart view with all items
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> Cart()
    {
        var userId = _userManager.GetUserId(User);

        var order = await _context
            .Orders.Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.UserId == userId && o.Status == "Pending");

        return View(order);
    }

    // Update quantity of an item in the cart
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> UpdateQuantity(int orderItemId, int quantity)
    {
        if (quantity <= 0)
        {
            TempData["Error"] = "Quantity must be at least 1.";
            return RedirectToAction(nameof(Cart));
        }

        var userId = _userManager.GetUserId(User);

        var item = await _context
            .OrderItems.Include(oi => oi.Order)
            .FirstOrDefaultAsync(oi => oi.Id == orderItemId);

        if (item == null || item.Order == null)
        {
            TempData["Error"] = "Cart item not found.";
            return RedirectToAction(nameof(Cart));
        }

        if (item.Order.UserId != userId)
            return Forbid();

        item.Quantity = quantity;

        await RecalculateTotal(item.OrderId);

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

        var item = await _context
            .OrderItems.Include(oi => oi.Order)
            .FirstOrDefaultAsync(oi => oi.Id == orderItemId);

        if (item == null || item.Order == null)
        {
            TempData["Error"] = "Item not found.";
            return RedirectToAction(nameof(Cart));
        }

        if (item.Order.UserId != userId)
            return Forbid();

        _context.OrderItems.Remove(item);

        await RecalculateTotal(item.OrderId);

        TempData["Success"] = "Item removed from cart.";
        return RedirectToAction(nameof(Cart));
    }

    // Checkout for finalizing the order
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> Checkout()
    {
        var userId = _userManager.GetUserId(User);

        var order = await _context
            .Orders.Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.UserId == userId && o.Status == "Pending");

        if (order == null || !order.OrderItems.Any())
        {
            TempData["Error"] = "Your cart is empty.";
            return RedirectToAction(nameof(Cart));
        }

        order.Status = "Processing";
        order.CreatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Order placed successfully.";

        return RedirectToAction(nameof(Details), new { id = order.Id });
    }

    // Order details view for both customers and admins
    [Authorize(Roles = "Customer,Admin")]
    public async Task<IActionResult> Details(int id)
    {
        var userId = _userManager.GetUserId(User);

        var order = await _context
            .Orders.Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            TempData["Error"] = "Order not found.";
            return RedirectToAction("Index", "Home");
        }

        if (User.IsInRole("Customer") && order.UserId != userId)
            return Forbid();

        return View(order);
    }

    // Customer's order history (excluding pending orders)
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> MyOrders()
    {
        var userId = _userManager.GetUserId(User);

        var orders = await _context
            .Orders.Where(o => o.UserId == userId && o.Status != "Pending")
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return View(orders);
    }

    // Admin index view of all orders with management capabilities
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminIndex()
    {
        var orders = await _context
            .Orders.Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return View(orders);
    }

    // Admin update order status (e.g. from Processing to Completed)
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        var order = await _context.Orders.FindAsync(id);

        if (order == null)
        {
            TempData["Error"] = "Order not found.";
            return RedirectToAction(nameof(AdminIndex));
        }

        var allowedStatuses = new[] { "Pending", "Processing", "Completed", "Cancelled" };

        if (!allowedStatuses.Contains(status))
        {
            TempData["Error"] = "Invalid status value.";
            return RedirectToAction(nameof(AdminIndex));
        }

        order.Status = status;

        await _context.SaveChangesAsync();

        TempData["Success"] = $"Order #{order.Id} status updated to {status}.";
        return RedirectToAction(nameof(AdminIndex));
    }

    // Helper method to recalculate total amount of an order based on its items
    private async Task RecalculateTotal(int orderId)
    {
        var total = await _context
            .OrderItems.Where(oi => oi.OrderId == orderId)
            .SumAsync(oi => oi.Quantity * oi.UnitPrice);

        var order = await _context.Orders.FindAsync(orderId);

        if (order != null)
        {
            order.TotalAmount = total;
            await _context.SaveChangesAsync();
        }
    }
}
