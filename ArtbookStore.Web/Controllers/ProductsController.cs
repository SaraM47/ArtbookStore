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

namespace ArtbookStore.Web.Controllers
{
    // Public read access for product listing and details
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Public access to product listing and details, admin access required for create/edit/delete
        // GET: Products
        [AllowAnonymous] // Public access allowed
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Products.Include(p => p.Category);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Products/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
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

        // Admin only access for create/edit/delete operations
        // GET: Products/Create
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

            return RedirectToAction(nameof(Index));
        }

        // GET: Products/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var product = await _context.Products.FindAsync(id);

            if (product == null)
                return NotFound();

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

            var product = await _context.Products.FindAsync(id);

            if (product == null)
                return NotFound();

            product.Title = model.Title;
            product.Author = model.Author;
            product.Description = model.Description;
            product.Price = model.Price;
            product.ImageUrl = model.ImageUrl;
            product.StockQuantity = model.StockQuantity;
            product.CategoryId = model.CategoryId;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Products/Delete/5
        [Authorize(Roles = "Admin")]
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
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Helper method to check if a product exists by ID
        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }

        /// <summary>
        /// Populates category dropdown for Create/Edit views.
        /// Uses CategoryId as value and Name as display text.
        /// </summary>
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
    }
}
