using Booki.DataAccess.Context;
using Booki.DataAccess.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace booki.Controllers;

[Authorize]
public class FavoritesController(BookiDbContext context) : Controller
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int bookId, string? returnUrl)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return Challenge();
        }

        var bookExists = await context.Books.AnyAsync(book => book.Id == bookId);

        if (!bookExists)
        {
            return NotFound();
        }

        var favorite = await context.Favorites
            .FirstOrDefaultAsync(f => f.UserId == userId && f.BookId == bookId);

        if (favorite == null)
        {
            context.Favorites.Add(new Favorite
            {
                UserId = userId,
                BookId = bookId
            });
        }
        else
        {
            context.Favorites.Remove(favorite);
        }

        await context.SaveChangesAsync();

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction("Details", "Books", new { id = bookId });
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return Challenge();
        }

        var favorites = await context.Favorites
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .Include(f => f.Book).ThenInclude(b => b!.Author)
            .Include(f => f.Book).ThenInclude(b => b!.Category)
            .ToListAsync();

        var books = favorites
            .Where(f => f.Book != null)
            .Select(f => f.Book!)
            .ToList();

        return View(books);
    }
}