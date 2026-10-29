using Booki.DataAccess.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Booki.DataAccess.Entities;

namespace booki.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ChaptersController(BookiDbContext context) : Controller
{
    public async Task<IActionResult> Index(int bookId)
    {
        var book = await context.Books.FindAsync(bookId);

        if (book == null)
        {
            return NotFound();
        }

        var chapters = await context.BookChapters
            .Where(chapter => chapter.BookId == bookId)
            .OrderBy(chapter => chapter.OrderNumber)
            .ToListAsync();

        ViewBag.Book = book;

        return View(chapters);
    }

    [HttpGet]
    public async Task<IActionResult> Create(int bookId)
    {
        var book = await context.Books.FindAsync(bookId);

        if (book == null)
        {
            return NotFound();
        }

        var lastOrder = await context.BookChapters
            .Where(chapter => chapter.BookId == bookId)
            .MaxAsync(chapter => (int?)chapter.OrderNumber) ?? 0;

        ViewBag.Book = book;

        return View(new BookChapter
        {
            BookId = bookId,
            OrderNumber = lastOrder + 1
        });
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookChapter chapter)
    {
        var book = await context.Books.FindAsync(chapter.BookId);

        if (book == null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(chapter.Title))
        {
            ModelState.AddModelError("Title", "Bölüm başlığı zorunludur.");
        }

        if (string.IsNullOrWhiteSpace(chapter.Content))
        {
            ModelState.AddModelError("Content", "Bölüm metni zorunludur.");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Book = book;
            return View(chapter);
        }

        chapter.Title = chapter.Title.Trim();

        context.BookChapters.Add(chapter);
        await context.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { bookId = chapter.BookId });
    }
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var chapter = await context.BookChapters
            .Include(c => c.Book)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (chapter == null)
        {
            return NotFound();
        }

        ViewBag.Book = chapter.Book;

        return View(chapter);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, BookChapter chapter)
    {
        if (id != chapter.Id)
        {
            return NotFound();
        }

        var existingChapter = await context.BookChapters
            .Include(c => c.Book)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (existingChapter == null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(chapter.Title))
        {
            ModelState.AddModelError("Title", "Bölüm başlığı zorunludur.");
        }

        if (string.IsNullOrWhiteSpace(chapter.Content))
        {
            ModelState.AddModelError("Content", "Bölüm metni zorunludur.");
        }

        if (!ModelState.IsValid)
        {
            chapter.BookId = existingChapter.BookId;
            ViewBag.Book = existingChapter.Book;
            return View(chapter);
        }

        existingChapter.OrderNumber = chapter.OrderNumber;
        existingChapter.Title = chapter.Title.Trim();
        existingChapter.Content = chapter.Content;

        await context.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { bookId = existingChapter.BookId });
    }
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var chapter = await context.BookChapters
            .Include(c => c.Book)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (chapter == null)
        {
            return NotFound();
        }

        return View(chapter);
    }
}
