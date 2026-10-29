using Booki.DataAccess.Context;
using Booki.DataAccess.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace booki.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class BooksController(BookiDbContext context) : Controller
{
    public async Task<IActionResult> Index()
    {
        var books = await context.Books
            .Include(book => book.Author)
            .Include(book => book.Category)
            .OrderBy(book => book.Title)
            .ToListAsync();

        return View(books);
    }
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.Authors = new SelectList(
            await context.Authors.OrderBy(author => author.FullName).ToListAsync(),
            "Id",
            "FullName");

        ViewBag.Categories = new SelectList(
            await context.Categories.OrderBy(category => category.Name).ToListAsync(),
            "Id",
            "Name");

        return View();
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Book book)
    {
        if (string.IsNullOrWhiteSpace(book.Title))
        {
            ModelState.AddModelError("Title", "Başlık zorunludur.");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Authors = new SelectList(
                await context.Authors.OrderBy(author => author.FullName).ToListAsync(),
                "Id",
                "FullName",
                book.AuthorId);

            ViewBag.Categories = new SelectList(
                await context.Categories.OrderBy(category => category.Name).ToListAsync(),
                "Id",
                "Name",
                book.CategoryId);

            return View(book);
        }

        book.Title = book.Title.Trim();

        context.Books.Add(book);
        await context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var book = await context.Books.FindAsync(id);

        if (book == null)
        {
            return NotFound();
        }

        ViewBag.Authors = new SelectList(
            await context.Authors.OrderBy(author => author.FullName).ToListAsync(),
            "Id",
            "FullName",
            book.AuthorId);

        ViewBag.Categories = new SelectList(
            await context.Categories.OrderBy(category => category.Name).ToListAsync(),
            "Id",
            "Name",
            book.CategoryId);

        return View(book);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Book book)
    {
        if (id != book.Id)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(book.Title))
        {
            ModelState.AddModelError("Title", "Başlık zorunludur.");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Authors = new SelectList(
                await context.Authors.OrderBy(author => author.FullName).ToListAsync(),
                "Id",
                "FullName",
                book.AuthorId);

            ViewBag.Categories = new SelectList(
                await context.Categories.OrderBy(category => category.Name).ToListAsync(),
                "Id",
                "Name",
                book.CategoryId);

            return View(book);
        }

        var existingBook = await context.Books.FindAsync(id);

        if (existingBook == null)
        {
            return NotFound();
        }

        existingBook.Title = book.Title.Trim();
        existingBook.Summary = book.Summary;
        existingBook.PublishedYear = book.PublishedYear;
        existingBook.CoverImageUrl = book.CoverImageUrl;
        existingBook.AuthorId = book.AuthorId;
        existingBook.CategoryId = book.CategoryId;
        existingBook.SourceName = book.SourceName;
        existingBook.SourceUrl = book.SourceUrl;
        existingBook.CopyrightStatus = book.CopyrightStatus;
        existingBook.Translator = book.Translator;
        existingBook.TranslationCopyrightStatus = book.TranslationCopyrightStatus;

        await context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var book = await context.Books
            .Include(b => b.Author)
            .Include(b => b.Category)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (book == null)
        {
            return NotFound();
        }

        return View(book);
    }
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var book = await context.Books.FindAsync(id);

        if (book == null)
        {
            return NotFound();
        }

        context.Books.Remove(book);
        await context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}