using Booki.DataAccess.Context;
using Booki.DataAccess.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace booki.Controllers;

[Authorize]
public class CommentsController(BookiDbContext context) : Controller
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int bookId, string? content)
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

        content = content?.Trim();

        if (string.IsNullOrWhiteSpace(content))
        {
            TempData["CommentError"] = "Yorum boş olamaz.";
        }
        else if (content.Length > 1000)
        {
            TempData["CommentError"] = "Yorum en fazla 1000 karakter olabilir.";
        }
        else
        {
            context.Comments.Add(new Comment
            {
                UserId = userId,
                BookId = bookId,
                Content = content
            });

            await context.SaveChangesAsync();
        }

        return RedirectToAction("Details", "Books", new { id = bookId });
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

        var comment = await context.Comments.FindAsync(id);

        if (comment == null)
        {
            return NotFound();
        }

        if (comment.UserId != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var bookId = comment.BookId;

        context.Comments.Remove(comment);
        await context.SaveChangesAsync();

        return RedirectToAction("Details", "Books", new { id = bookId });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return Challenge();
        }

        var comment = await context.Comments
            .Include(c => c.Book)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (comment == null)
        {
            return NotFound();
        }

        if (comment.UserId != userId)
        {
            return Forbid();
        }

        return View(comment);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, string? content)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return Challenge();
        }

        var comment = await context.Comments.FindAsync(id);

        if (comment == null)
        {
            return NotFound();
        }

        if (comment.UserId != userId)
        {
            return Forbid();
        }

        content = content?.Trim();

        if (string.IsNullOrWhiteSpace(content))
        {
            TempData["CommentError"] = "Yorum boş olamaz.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        if (content.Length > 1000)
        {
            TempData["CommentError"] = "Yorum en fazla 1000 karakter olabilir.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        comment.Content = content;
        comment.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();

        return RedirectToAction("Details", "Books", new { id = comment.BookId });
    }
}