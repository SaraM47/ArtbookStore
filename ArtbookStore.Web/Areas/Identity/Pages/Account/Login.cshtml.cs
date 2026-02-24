using System.ComponentModel.DataAnnotations;
using ArtbookStore.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace ArtbookStore.Web.Areas.Identity.Pages.Account
{
    // Handles login logic for both Admin and Customer users. After successful login, redirects Admins to the Admin Dashboard and Customers to the Home page.
    public class LoginModel : PageModel
    {
        // SignInManager performs the actual sign-in (cookie creation, password check, etc.)
        private readonly SignInManager<ApplicationUser> _signInManager;

        // UserManager is used to fetch the user and check roles.
        private readonly UserManager<ApplicationUser> _userManager;

        // Logger is optional but useful for tracking login events and issues.
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<LoginModel> logger
        )
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        // InputModel represents the data submitted by the user in the login form.
        [BindProperty]
        public InputModel Input { get; set; } = new();

        // ReturnUrl uses to redirect the user back to the page they originally wanted to access after a successful login. This is optional and can be null.
        public string? ReturnUrl { get; set; }

        // The InputModel class defines the properties for the login form, including validation attributes to ensure that the user provides a valid email and password. These field must not be empty and the email must be in a valid format. The BindProperty attribute allows these properties to be automatically populated from the form submission.
        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = "";

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; } = "";
        }

        // GET: /Identity/Account/Login, show the login form.
        public void OnGet(string? returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }

        // ======================================================
        // POST: Faktisk inloggning
        // ======================================================
        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            ReturnUrl = returnUrl;

            // Server-side validation check. If validation fails, re-render the page and show errors.
            if (!ModelState.IsValid)
                return Page();

            // Try to sign in the user with the provided email and password. This checks the credentials and creates the authentication cookie if successful.
            var result = await _signInManager.PasswordSignInAsync(
                Input.Email,
                Input.Password,
                isPersistent: false,
                lockoutOnFailure: false
            );

            // If login fails, show a generic error message.
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }

            // Fetch the user from database so we can check roles.
            var user = await _userManager.FindByEmailAsync(Input.Email);

            if (user == null)
            {
                // Safty check: This should not happen because the sign-in succeeded, but if it does, log an error and redirect to home.
                return RedirectToAction("Index", "Home");
            }

            // Rollbased redirect logic after successful login:

            // Admin will be redirected to the Admin Dashboard (AdminController's Index action)
            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return RedirectToAction("Index", "Admin");
            }

            // Customer will be redirected to the Home page (HomeController's Index action)
            return RedirectToAction("Index", "Home");
        }
    }
}
