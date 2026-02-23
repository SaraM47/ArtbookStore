using System.Diagnostics;
using ArtbookStore.Web.Data;
using ArtbookStore.Web.Models;
using ArtbookStore.Web.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArtbookStore.Web.Controllers;

// This controller handles the homepage, public information pages, the contact form, system and error pages
public class HomeController : Controller
{
    // Database context used to access data from the database
    private readonly ApplicationDbContext _context;

    // The constructor takes the database context as a parameter, which is automatically provided (injected) when the controller is created.
    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Home page and static pages retrieving data 3 latest products and 3 categories for the homepage and sending it to the HomeViewModel.
    public async Task<IActionResult> Index()
    {
        // Create a ViewModel to send structured data to the View
        var vm = new HomeViewModel
        {
            // Get the 3 most recently created products
            FeaturedProducts = await _context
                .Products.OrderByDescending(p => p.CreatedAt) // Order by creation date, newest first
                .Take(3) // Only take the top 3 products
                .ToListAsync(),

            // Get the first 3 categories ordered by their ID
            Categories = await _context.Categories.OrderBy(c => c.CategoryId).Take(3).ToListAsync(),
        };

        return View(vm);
    }

    // About-page shows static information about the store that is no dynamic data needed, so we just return the view by sending an empty ContactViewModel to the View.
    public IActionResult About()
    {
        return View();
    }

    // Contact-page shows a contact form. The GET action returns the view with an empty ContactViewModel, while the POST action handles form submission, validates the input, and either redisplays the form with errors or shows a success message using TempData and redirects back to the GET action (PRG pattern).
    [HttpGet]
    public IActionResult Contact()
    {
        return View(new ContactViewModel());
    }

    // Handles the form submission from the Contact page. It performs server-side validation and, if valid, can later be extended to send an email, save to a database, or log the inquiry. After processing, it sets a success message in TempData and redirects back to the Contact page using the Post-Redirect-Get (PRG) pattern to prevent duplicate submissions on page refresh.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Contact(ContactViewModel model)
    {
        // Server-side validation of the form input. If the model state is invalid (e.g., required fields are missing), it redisplays the form with validation errors.
        if (!ModelState.IsValid)
            return View(model);

        // Store a temporary success message
        TempData["Success"] = "Your message has been sent successfully.";

        // Prevents duplicate form submission if the user refreshes the page
        return RedirectToAction(nameof(Contact));
    }

    // System pages that show static information about the store, such as privacy policy, terms and conditions, and FAQ. These actions simply return their respective views.
    public IActionResult Privacy()
    {
        return View();
    }

    // Terms and conditions page.
    public IActionResult Terms()
    {
        return View();
    }

    // FAQ-page
    public IActionResult FAQ()
    {
        return View();
    }

    // Access denied page that is shown when a user tries to access a restricted area without proper authorization. This action simply returns the AccessDenied view.
    public IActionResult AccessDenied()
    {
        return View();
    }

    // Handels HTTP status codes like 404 (Not Found) and other errors. If the code is 404, it returns a custom NotFound view. For any other status code, it returns a generic Error view.
    public IActionResult HttpStatusCodeHandler(int code)
    {
        if (code == 404)
            return View("NotFound");

        return View("Error");
    }

    // General error page that response caching is disabled to ensure correct error handling.
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(
            new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier }
        );
    }
}
