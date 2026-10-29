using Booki.DataAccess.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Booki.DataAccess.Entities;

namespace booki.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class CategoriesController(BookiDbContext context) : Controller
{
    public async Task<IActionResult> Index()
    {
        var categories = await context.Categories
            .OrderBy(category => category.Name)
            .ToListAsync();

        return View(categories);
    }
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Category category)
    {
        if (string.IsNullOrWhiteSpace(category.Name))
        {
            ModelState.AddModelError("Name", "Kategori adı zorunludur.");
        }

        if (!ModelState.IsValid)
        {
            return View(category);
        }

        category.Name = category.Name.Trim();

        context.Categories.Add(category);
        await context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var category = await context.Categories.FindAsync(id);

        if (category == null)
        {
            return NotFound();
        }

        return View(category);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Category category)
    {
        if (id != category.Id)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(category.Name))
        {
            ModelState.AddModelError("Name", "Kategori adı zorunludur.");
        }

        if (!ModelState.IsValid)
        {
            return View(category);
        }

        var existingCategory = await context.Categories.FindAsync(id);

        if (existingCategory == null)
        {
            return NotFound();
        }

        existingCategory.Name = category.Name.Trim();
        existingCategory.Description = category.Description;

        await context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await context.Categories.FindAsync(id);

        if (category == null)
        {
            return NotFound();
        }

        return View(category);
    }
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var category = await context.Categories.FindAsync(id);

        if (category == null)
        {
            return NotFound();
        }

        context.Categories.Remove(category);
        await context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}