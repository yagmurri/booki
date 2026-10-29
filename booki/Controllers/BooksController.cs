using Booki.DataAccess.Context;
using Booki.DataAccess.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Booki.DataAccess.Entities;
using System.Security.Claims;

namespace booki.Controllers;

public class BooksController(BookiDbContext context) : Controller
{
    public async Task<IActionResult> Index(string? search, int? categoryId, int? authorId)
    {
        var query = context.Books
            .Include(book => book.Author)
            .Include(book => book.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            query = query.Where(book =>
                book.Title.Contains(search) ||
                (book.Author != null && book.Author.FullName.Contains(search)));
        }

        if (categoryId != null)
        {
            query = query.Where(book => book.CategoryId == categoryId);
        }

        if (authorId != null)
        {
            query = query.Where(book => book.AuthorId == authorId);
        }

        var books = await query
            .OrderBy(book => book.Title)
            .ToListAsync();

        ViewBag.Search = search;
        ViewBag.SelectedCategoryId = categoryId;
        ViewBag.SelectedAuthorId = authorId;
        ViewBag.Categories = await context.Categories
            .OrderBy(category => category.Name)
            .ToListAsync();
        ViewBag.Authors = await context.Authors
            .OrderBy(author => author.FullName)
            .ToListAsync();

        return View(books);
    }
    public async Task<IActionResult> Details(int id)
    {
        var book = await context.Books
            .Include(b => b.Author)
            .Include(b => b.Category)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (book == null)
        {
            return NotFound();
        }

        var isFavorite = false;
        var hasProgress = false;
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId != null)
        {
            isFavorite = await context.Favorites
                .AnyAsync(f => f.UserId == userId && f.BookId == id);

            hasProgress = await context.ReadingProgresses
                .AnyAsync(p => p.UserId == userId && p.BookId == id);
        }

        ViewBag.IsFavorite = isFavorite;
        ViewBag.HasProgress = hasProgress;

        return View(book);
    }
    public async Task<IActionResult> Read(int id, int? chapter)
    {
        var book = await context.Books
            .Include(b => b.Author)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (book == null)
        {
            return NotFound();
        }

        var chapters = await context.BookChapters
            .Where(c => c.BookId == id)
            .OrderBy(c => c.OrderNumber)
            .Select(c => new BookChapter
            {
                Id = c.Id,
                BookId = c.BookId,
                OrderNumber = c.OrderNumber,
                Title = c.Title
            })
            .ToListAsync();

        if (!chapters.Any())
        {
            return RedirectToAction(nameof(Details), new { id });
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        ReadingProgress? progress = null;

        if (userId != null)
        {
            progress = await context.ReadingProgresses
                .FirstOrDefaultAsync(p => p.UserId == userId && p.BookId == id);
        }

        BookChapter? currentInfo;

        if (chapter != null)
        {
            currentInfo = chapters.FirstOrDefault(c => c.Id == chapter);

            if (currentInfo == null)
            {
                return NotFound();
            }
        }
        else
        {
            currentInfo = progress != null
                ? chapters.FirstOrDefault(c => c.Id == progress.ChapterId)
                : null;

            currentInfo ??= chapters.First();
        }

        if (userId != null)
        {
            if (progress == null)
            {
                context.ReadingProgresses.Add(new ReadingProgress
                {
                    UserId = userId,
                    BookId = id,
                    ChapterId = currentInfo.Id
                });
            }
            else
            {
                progress.ChapterId = currentInfo.Id;
                progress.UpdatedAt = DateTime.UtcNow;
            }

            await context.SaveChangesAsync();
        }

        var current = await context.BookChapters.FindAsync(currentInfo.Id);

        var index = chapters.IndexOf(currentInfo);

        ViewBag.Book = book;
        ViewBag.Chapters = chapters;
        ViewBag.PreviousChapterId = index > 0 ? chapters[index - 1].Id : (int?)null;
        ViewBag.NextChapterId = index < chapters.Count - 1 ? chapters[index + 1].Id : (int?)null;

        return View(current);
    }
}