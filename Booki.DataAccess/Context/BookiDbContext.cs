using Booki.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Booki.DataAccess.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
namespace Booki.DataAccess.Context;

public class BookiDbContext : IdentityDbContext<ApplicationUser>
{
    public BookiDbContext(DbContextOptions<BookiDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Author> Authors => Set<Author>();

    public DbSet<Book> Books => Set<Book>();

    public DbSet<BookChapter> BookChapters => Set<BookChapter>();

    public DbSet<Favorite> Favorites => Set<Favorite>();

    public DbSet<ReadingProgress> ReadingProgresses => Set<ReadingProgress>();

    public DbSet<Bookmark> Bookmarks => Set<Bookmark>();

    public DbSet<Comment> Comments => Set<Comment>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Favorite>()
            .HasIndex(favorite => new { favorite.UserId, favorite.BookId })
            .IsUnique();

        builder.Entity<ReadingProgress>()
    .HasIndex(progress => new { progress.UserId, progress.BookId })
    .IsUnique();

        builder.Entity<Bookmark>()
    .HasIndex(bookmark => new { bookmark.UserId, bookmark.ChapterId })
    .IsUnique();

        builder.Entity<Comment>()
    .Property(comment => comment.Content)
    .HasMaxLength(1000);

        builder.Entity<Comment>()
    .HasIndex(comment => comment.BookId);
    }



}