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
}