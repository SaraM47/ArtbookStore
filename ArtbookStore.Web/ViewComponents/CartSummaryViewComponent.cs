using ArtbookStore.Web.Data;
using ArtbookStore.Web.Models;
using ArtbookStore.Web.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

/*
* ViewComponent to display a summary of the shopping cart in the header. Shows the number of items and total amount. If the user is not logged in or does not have an active order, an empty shopping cart is displayed.
*/
namespace ArtbookStore.Web.ViewComponents
{
    public class CartSummaryViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartSummaryViewComponent(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager
        )
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Always return a ViewModel, even if it's empty, to avoid null reference issues in the view
            var vm = new CartSummaryViewModel();

            // If not logged in, return empty cart
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return View("Default", vm);
            }

            var user = await _userManager.GetUserAsync(UserClaimsPrincipal);

            if (user == null)
            {
                return View("Default", vm);
            }

            var order = await _context
                .Orders.Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.UserId == user.Id && o.Status == "Pending");

            if (order == null)
            {
                return View("Default", vm);
            }

            // Map to ViewModel
            vm.ItemCount = order.OrderItems.Sum(i => i.Quantity);
            vm.TotalAmount = order.TotalAmount;

            vm.Items = order
                .OrderItems.Select(i => new CartItemDto
                {
                    Title = i.Product?.Title,
                    Quantity = i.Quantity,
                })
                .ToList();

            return View("Default", vm);
        }
    }
}
