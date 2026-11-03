using Booki.DataAccess.Context;
using Booki.DataAccess.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace booki.Controllers;

[Authorize]
public class QuotesController(BookiDbContext context) : Controller
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int chapterId, string? text)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return Challenge();
        }

        var chapter = await context.BookChapters
            .Where(c => c.Id == chapterId)
            .Select(c => new { c.Id, c.BookId })
            .FirstOrDefaultAsync();

        if (chapter == null)
        {
            return NotFound();
        }

        text = text?.Trim();

        if (string.IsNullOrWhiteSpace(text))
        {
            TempData["QuoteMessage"] = "Alıntı boş olamaz.";
        }
        else if (text.Length > 1000)
        {
            TempData["QuoteMessage"] = "Alıntı en fazla 1000 karakter olabilir.";
        }
        else
        {
            context.Quotes.Add(new Quote
            {
                UserId = userId,
                BookId = chapter.BookId,
                ChapterId = chapter.Id,
                Text = text
            });

            await context.SaveChangesAsync();

            TempData["QuoteMessage"] = "Alıntı kaydedildi.";
        }

        return RedirectToAction("Read", "Books", new { id = chapter.BookId, chapter = chapter.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return Challenge();
        }

        var quotes = await context.Quotes
            .Where(q => q.UserId == userId)
            .OrderByDescending(q => q.CreatedAt)
            .Include(q => q.Book).ThenInclude(b => b!.Author)
            .ToListAsync();

        quotes = quotes
            .Where(q => q.Book != null)
            .ToList();

        var chapterIds = quotes.Select(q => q.ChapterId).ToList();

        var chapterTitles = await context.BookChapters
            .Where(c => chapterIds.Contains(c.Id))
            .Select(c => new { c.Id, c.Title, c.OrderNumber })
            .ToDictionaryAsync(c => c.Id);

        quotes = quotes
            .Where(q => chapterTitles.ContainsKey(q.ChapterId))
            .ToList();

        ViewBag.ChapterTitles = chapterTitles.ToDictionary(
            c => c.Key,
            c => $"{c.Value.OrderNumber}. {c.Value.Title}");

        return View(quotes);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return Challenge();
        }

        var quote = await context.Quotes.FindAsync(id);

        if (quote == null)
        {
            return NotFound();
        }

        if (quote.UserId != userId)
        {
            return Forbid();
        }

        context.Quotes.Remove(quote);
        await context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}
