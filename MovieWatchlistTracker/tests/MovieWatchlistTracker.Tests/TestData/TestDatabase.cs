using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MovieWatchlistTracker.Web.Data;
using MovieWatchlistTracker.Web.Models;

namespace MovieWatchlistTracker.Tests.TestData;

public sealed class TestDatabase : IDisposable
{
    public const string PrimaryUserId = "user-one";
    public const string OtherUserId = "user-two";

    private readonly SqliteConnection _connection;

    public TestDatabase()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        using var context = CreateContext();
        context.Database.EnsureCreated();
        Seed(context);
    }

    public ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        return new ApplicationDbContext(options);
    }

    public void Dispose()
    {
        _connection.Dispose();
    }

    private static void Seed(ApplicationDbContext context)
    {
        var primaryUser = new ApplicationUser
        {
            Id = PrimaryUserId,
            UserName = "primary.user",
            NormalizedUserName = "PRIMARY.USER",
            Email = "primary@example.test",
            NormalizedEmail = "PRIMARY@EXAMPLE.TEST",
            CreatedAt = DateTime.UtcNow
        };

        var otherUser = new ApplicationUser
        {
            Id = OtherUserId,
            UserName = "other.user",
            NormalizedUserName = "OTHER.USER",
            Email = "other@example.test",
            NormalizedEmail = "OTHER@EXAMPLE.TEST",
            CreatedAt = DateTime.UtcNow
        };

        var action = new Genre { Name = "Action" };
        var drama = new Genre { Name = "Drama" };

        var firstMovie = new Movie
        {
            Title = "Alpha Signal",
            ReleaseYear = 2024,
            Description = "A focused test movie.",
            Runtime = 101,
            PosterUrl = "/images/test/alpha.svg"
        };

        var secondMovie = new Movie
        {
            Title = "Beta Harbor",
            ReleaseYear = 2020,
            Description = "Another seeded test movie.",
            Runtime = 98,
            PosterUrl = "/images/test/beta.svg"
        };

        context.Users.AddRange(primaryUser, otherUser);
        context.Genres.AddRange(action, drama);
        context.Movies.AddRange(firstMovie, secondMovie);
        context.SaveChanges();

        context.MovieGenres.AddRange(
            new MovieGenre { MovieId = firstMovie.Id, GenreId = action.Id },
            new MovieGenre { MovieId = secondMovie.Id, GenreId = drama.Id });

        context.Watchlists.AddRange(
            new Watchlist
            {
                UserId = PrimaryUserId,
                Name = "My Watchlist",
                CreatedAt = DateTime.UtcNow
            },
            new Watchlist
            {
                UserId = OtherUserId,
                Name = "My Watchlist",
                CreatedAt = DateTime.UtcNow
            });

        context.SaveChanges();
    }
}
