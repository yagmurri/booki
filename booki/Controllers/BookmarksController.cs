using Booki.DataAccess.Context;
using Booki.DataAccess.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace booki.Controllers;

[Authorize]
public class BookmarksController(BookiDbContext context) : Controller
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int chapterId, string? returnUrl)
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

        var bookmark = await context.Bookmarks
            .FirstOrDefaultAsync(b => b.UserId == userId && b.ChapterId == chapterId);

        if (bookmark == null)
        {
            context.Bookmarks.Add(new Bookmark
            {
                UserId = userId,
                BookId = chapter.BookId,
                ChapterId = chapter.Id
            });
        }
        else
        {
            context.Bookmarks.Remove(bookmark);
        }

        await context.SaveChangesAsync();

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
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

        var bookmarks = await context.Bookmarks
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .Include(b => b.Book).ThenInclude(book => book!.Author)
            .ToListAsync();

        bookmarks = bookmarks
            .Where(b => b.Book != null)
            .ToList();

        var chapterIds = bookmarks.Select(b => b.ChapterId).ToList();

        var chapterTitles = await context.BookChapters
            .Where(c => chapterIds.Contains(c.Id))
            .Select(c => new { c.Id, c.Title, c.OrderNumber })
            .ToDictionaryAsync(c => c.Id);

        bookmarks = bookmarks
            .Where(b => chapterTitles.ContainsKey(b.ChapterId))
            .ToList();

        ViewBag.ChapterTitles = chapterTitles.ToDictionary(
            c => c.Key,
            c => $"{c.Value.OrderNumber}. {c.Value.Title}");

        return View(bookmarks);
    }
}