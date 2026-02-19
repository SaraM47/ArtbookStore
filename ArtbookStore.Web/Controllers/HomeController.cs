using System.Diagnostics;
using ArtbookStore.Web.Data;
using ArtbookStore.Web.Models;
using ArtbookStore.Web.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArtbookStore.Web.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var vm = new HomeViewModel
        {
            FeaturedProducts = await _context
                .Products.OrderByDescending(p => p.CreatedAt)
                .Take(3)
                .ToListAsync(),

            Categories = await _context.Categories.OrderBy(c => c.CategoryId).Take(3).ToListAsync(),
        };

        return View(vm);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult AccessDenied()
    {
        return View();
    }

    public IActionResult HttpStatusCodeHandler(int code)
    {
        if (code == 404)
            return View("NotFound");

        return View("Error");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(
            new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier }
        );
    }
}
