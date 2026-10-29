using Booki.DataAccess.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace booki.Controllers;

[Authorize]
public class ReadingController(BookiDbContext context) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return Challenge();
        }

        var progresses = await context.ReadingProgresses
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.UpdatedAt)
            .Include(p => p.Book).ThenInclude(b => b!.Author)
            .Include(p => p.Book).ThenInclude(b => b!.Category)
            .ToListAsync();

        progresses = progresses
            .Where(p => p.Book != null)
            .ToList();

        return View(progresses);
    }
}