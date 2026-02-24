using ArtbookStore.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArtbookStore.Web.Areas.Identity.Pages.Account
{
    // AllowAnonymous means even non-logged-in users can reach this page.
    // We still control what happens in OnGet and OnPost.
    [AllowAnonymous]
    public class LogoutModel : PageModel
    {
        // Handles logout of both Admin and Customer. After logout, Admins are redirected to the Login page and Customers to the Home page. SignInManager is responsible for signing out (clearing auth cookie, etc.)
        private readonly SignInManager<ApplicationUser> _signInManager;

        public LogoutModel(SignInManager<ApplicationUser> signInManager)
        {
            _signInManager = signInManager;
        }

        // GET: /Identity/Account/Logout.
        // This method is called when the user navigates to the logout page. We don't want to log out immediately on GET  so we just redirect them back to the Home page. The actual logout happens in the POST method when they submit the logout form.
        public IActionResult OnGet()
        {
            return RedirectToAction("Index", "Home");
        }

        // POST: Perform the actual logout
        // This method is called when the logout form is submitted, determine what role the user has, then log out and redirect based on role.
        public async Task<IActionResult> OnPost()
        {
            // Check if the user is Admin before logout
            var isAdmin = User.IsInRole("Admin");

            // Log out the user (clear auth cookie, etc.)
            await _signInManager.SignOutAsync();

            // Rollbased redirect-logic
            // Admin redirects to Login page.
            if (isAdmin)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            // Customer redirects to Home page.
            return RedirectToAction("Index", "Home");
        }
    }
}
