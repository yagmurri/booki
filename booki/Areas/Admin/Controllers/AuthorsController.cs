using Booki.DataAccess.Context;
using Booki.DataAccess.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace booki.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class AuthorsController(BookiDbContext context) : Controller
{
    public async Task<IActionResult> Index()
    {
        var authors = await context.Authors
            .OrderBy(author => author.FullName)
            .ToListAsync();

        return View(authors);
    }
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Author author)
    {
        if (string.IsNullOrWhiteSpace(author.FullName))
        {
            ModelState.AddModelError("FullName", "Ad Soyad zorunludur.");
        }

        if (!ModelState.IsValid)
        {
            return View(author);
        }

        author.FullName = author.FullName.Trim();

        context.Authors.Add(author);
        await context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var author = await context.Authors.FindAsync(id);

        if (author == null)
        {
            return NotFound();
        }

        return View(author);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Author author)
    {
        if (id != author.Id)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(author.FullName))
        {
            ModelState.AddModelError("FullName", "Ad Soyad zorunludur.");
        }

        if (!ModelState.IsValid)
        {
            return View(author);
        }

        var existingAuthor = await context.Authors.FindAsync(id);

        if (existingAuthor == null)
        {
            return NotFound();
        }

        existingAuthor.FullName = author.FullName.Trim();
        existingAuthor.BirthYear = author.BirthYear;
        existingAuthor.DeathYear = author.DeathYear;
        existingAuthor.Biography = author.Biography;

        await context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var author = await context.Authors.FindAsync(id);

        if (author == null)
        {
            return NotFound();
        }

        return View(author);
    }
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var author = await context.Authors.FindAsync(id);

        if (author == null)
        {
            return NotFound();
        }

        context.Authors.Remove(author);
        await context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}