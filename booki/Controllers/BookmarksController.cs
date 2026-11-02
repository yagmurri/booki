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
}