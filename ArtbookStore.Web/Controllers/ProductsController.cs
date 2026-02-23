using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArtbookStore.Web.Data;
using ArtbookStore.Web.Models;
using ArtbookStore.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

// Controller for managing products. Public access for listing and details, admin access for create/edit/delete.
namespace ArtbookStore.Web.Controllers
{
    // Public read access for product listing and details
    public class ProductsController : Controller
    {
        // Database context used to access the database
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Public access to product listing and details, admin access required for create/edit/delete
        // GET: Products
        [AllowAnonymous] // Anyone can view product list
        public async Task<IActionResult> Index(string? category, int page = 1)
        {
            const int pageSize = 8; // Number of products per page

            // Start building a query
            // Include Category so we can access category data in the view
            var query = _context.Products.Include(p => p.Category).AsQueryable();

            // If category filter is provided, filter products
            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(p => p.Category != null && p.Category.Name == category);
            }

            // Count total products (after filtering)
            var totalProducts = await query.CountAsync();

            // Apply pagination and sorting
            var products = await query
                .OrderBy(p => p.Title) // Sort alphabetically
                .Skip((page - 1) * pageSize) // Skip previous pages
                .Take(pageSize) // Take only pageSize
                .ToListAsync();

            // Load all categories for filter buttons
            var categories = await _context.Categories.ToListAsync();

            // Send extra data to view using ViewBag
            ViewBag.Categories = categories;
            ViewBag.CurrentCategory = category;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);

            return View(products);
        }

        // Public access to product details
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            // Include Category for display purposes
            var product = await _context
                .Products.Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound();

            return View(product);
        }

        // Admin only access for create/edit/delete operations
        // GET: Create product by Products/Create
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            await PopulateCategoriesDropdown();
            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(ProductCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateCategoriesDropdown(model.CategoryId);
                return View(model);
            }

            // Map ViewModel to Product entity
            var product = new Product
            {
                Title = model.Title,
                Author = model.Author,
                Description = model.Description,
                Price = model.Price,
                ImageUrl = model.ImageUrl,
                StockQuantity = model.StockQuantity,
                CategoryId = model.CategoryId,
                CreatedAt = DateTime.UtcNow,
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Product created successfully.";

            return RedirectToAction(nameof(AdminIndex));
        }

        // Admin edit product by GET: Products/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var product = await _context.Products.FindAsync(id);

            if (product == null)
                return NotFound();

            // Map entity to ViewModel
            var model = new ProductEditViewModel
            {
                Id = product.Id,
                Title = product.Title,
                Author = product.Author,
                Description = product.Description,
                Price = product.Price,
                ImageUrl = product.ImageUrl,
                StockQuantity = product.StockQuantity,
                CategoryId = product.CategoryId,
            };

            await PopulateCategoriesDropdown(product.CategoryId);

            return View(model);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, ProductEditViewModel model)
        {
            if (id != model.Id)
                return NotFound();

            if (!ModelState.IsValid)
            {
                await PopulateCategoriesDropdown(model.CategoryId);
                return View(model);
            }

            // Find the existing product entity in the database
            var product = await _context.Products.FindAsync(id);

            if (product == null)
                return NotFound();

            // Update entity fields
            product.Title = model.Title;
            product.Author = model.Author;
            product.Description = model.Description;
            product.Price = model.Price;
            product.ImageUrl = model.ImageUrl;
            product.StockQuantity = model.StockQuantity;
            product.CategoryId = model.CategoryId;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Product updated successfully.";
            return RedirectToAction(nameof(AdminIndex));
        }

        // GET: Products/Delete/5
        [Authorize(Roles = "Admin")] // Only admins can access the delete confirmation page
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context
                .Products.Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Product deleted successfully.";
            return RedirectToAction(nameof(AdminIndex));
        }

        // Helper method to check if a product exists by ID
        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }

        // Admin product listing for populates category dropdown for Create/Edit views. Uses CategoryId as value and Name as display text.
        private async Task PopulateCategoriesDropdown(int? selectedCategory = null)
        {
            var categories = await _context.Categories.ToListAsync();

            ViewData["CategoryId"] = new SelectList(
                categories,
                "CategoryId",
                "Name",
                selectedCategory
            );
        }

        // Admin-only product listing with edit/delete links
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminIndex()
        {
            var products = await _context
                .Products.Include(p => p.Category)
                .OrderBy(p => p.Title)
                .ToListAsync();

            var categories = await _context.Categories.ToListAsync();
            ViewBag.Categories = categories;

            return View(products);
        }

        // AJAX endpoint to update stock quantity (Admin only)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStock(int id, int change)
        {
            // Find the product by ID
            var product = await _context.Products.FindAsync(id);

            // If product not found, return 404 Not Found
            if (product == null)
                return NotFound();

            // Update the stock quantity by adding the change (positive or negative)
            product.StockQuantity += change;

            // Ensure stock quantity does not go below zero to prevent negative stock
            if (product.StockQuantity < 0)
                product.StockQuantity = 0;

            await _context.SaveChangesAsync();

            // Return JSON response for AJAX call with the updated stock quantity
            return Json(new { stock = product.StockQuantity });
        }

        // Admin saves product changes (create or update) via AJAX. Validates input and updates database accordingly, returning JSON response.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Save(Product product)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            if (product.Id == 0)
            {
                // New product, set creation date and add to database
                product.CreatedAt = DateTime.UtcNow;
                _context.Products.Add(product);
            }
            else
            {
                // Update existing product
                var existing = await _context.Products.FindAsync(product.Id);
                if (existing == null)
                    return NotFound();

                existing.Title = product.Title;
                existing.Author = product.Author;
                existing.Price = product.Price;
                existing.StockQuantity = product.StockQuantity;
                existing.CategoryId = product.CategoryId;
                existing.ImageUrl = product.ImageUrl;
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Product saved successfully." });
        }
    }
}
