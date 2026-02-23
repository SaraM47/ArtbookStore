using System.Text.RegularExpressions;
using ArtbookStore.Web.Data;
using ArtbookStore.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArtbookStore.Web.Controllers;

// This controller is restricted to Admin users only, which is only users with the "Admin" role can access any action here.
[Authorize(Roles = "Admin")]
public class CategoriesController : Controller
{
    // Database context used to access the database.
    private readonly ApplicationDbContext _context;

    // The constructor takes the database context as a parameter, which is automatically provided (injected) when the controller is created.
    public CategoriesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Index action method shows the list of categories. It retrieves all categories from the database, ordered by name, and passes them to the view.
    public async Task<IActionResult> Index()
    {
        // Get all categories from the database
        // Order them alphabetically by Name
        var categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();

        return View(categories); // Send the list of categories to the View
    }

    /*
    * Save action method handles both creating a new category and updating an existing one. It checks if the model state is valid, generates a slug from the category name, and either adds a new category or updates an existing one in the database. After saving changes, it redirects back to the Index action.
    */
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(Category category)
    {
        // If the model is invalid (missing required fields etc.), redirect back to Index.
        if (!ModelState.IsValid)
            return RedirectToAction(nameof(Index));

        // Automatically generate a URL-friendly slug from the category name.
        category.Slug = GenerateSlug(category.Name ?? "");

        // If CategoryId is 0, this is a new category. Otherwise, it's an existing category that we need to update.
        if (category.CategoryId == 0)
        {
            _context.Categories.Add(category);
            TempData["Success"] = "Category created successfully.";
        }
        else
        {
            _context.Categories.Update(category);
            TempData["Success"] = "Category updated successfully.";
        }
        // Save changes to the database
        await _context.SaveChangesAsync();

        // Redirect back to the category list after saving
        return RedirectToAction(nameof(Index));
    }

    // Delete by removing category
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        // Find the category by its ID
        var category = await _context.Categories.FindAsync(id);

        if (category != null)
        {
            // Remove it from the database
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync(); // Save changes to the database

            TempData["Success"] = "Category deleted successfully.";
        }
        else
        {
            // If category was not found, set an error message
            TempData["Error"] = "Category not found.";
        }

        return RedirectToAction(nameof(Index)); // Redirect back to list
    }

    // Helper metod to generate a URL-friendly slug from the category name. It converts the name to lowercase, removes invalid characters, and replaces spaces with hyphens.
    private string GenerateSlug(string name)
    {
        var slug = name.ToLower().Trim();
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"\s+", "-");
        return slug;
    }
}
