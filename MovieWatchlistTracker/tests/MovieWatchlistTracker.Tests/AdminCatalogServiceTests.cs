using Microsoft.EntityFrameworkCore;
using MovieWatchlistTracker.Tests.TestData;
using MovieWatchlistTracker.Web.Models;
using MovieWatchlistTracker.Web.Services;
using MovieWatchlistTracker.Web.ViewModels;

namespace MovieWatchlistTracker.Tests;

public class AdminCatalogServiceTests
{
    [Fact]
    public async Task CreateMovieAsync_AddsMovieAndSelectedGenres()
    {
        using var database = new TestDatabase();
        await using var context = database.CreateContext();
        var service = new AdminCatalogService(context);
        var genreId = await context.Genres
            .Where(genre => genre.Name == "Action")
            .Select(genre => genre.Id)
            .SingleAsync();

        var result = await service.CreateMovieAsync(new AdminMovieFormViewModel
        {
            Title = "Gamma Station",
            ReleaseYear = 2026,
            Description = "A new admin-created movie.",
            Runtime = 120,
            PosterUrl = "/images/test/gamma.svg",
            ExternalApiId = "internal-gamma",
            SelectedGenreIds = [genreId]
        });

        Assert.True(result.Succeeded);

        var movie = await context.Movies
            .Include(movie => movie.MovieGenres)
            .SingleAsync(movie => movie.Title == "Gamma Station");

        Assert.Equal(2026, movie.ReleaseYear);
        Assert.Equal("internal-gamma", movie.ExternalApiId);
        Assert.Contains(movie.MovieGenres, movieGenre => movieGenre.GenreId == genreId);
    }

    [Fact]
    public async Task UpdateMovieAsync_RejectsDuplicateTitleAndReleaseYear()
    {
        using var database = new TestDatabase();
        await using var context = database.CreateContext();
        var service = new AdminCatalogService(context);
        var betaMovieId = await context.Movies
            .Where(movie => movie.Title == "Beta Harbor")
            .Select(movie => movie.Id)
            .SingleAsync();

        var result = await service.UpdateMovieAsync(new AdminMovieFormViewModel
        {
            Id = betaMovieId,
            Title = "Alpha Signal",
            ReleaseYear = 2024,
            Runtime = 90
        });

        Assert.False(result.Succeeded);
        Assert.Contains("same title and release year", result.Message);
    }

    [Fact]
    public async Task DeleteMovieAsync_BlocksWhenUserDataExists()
    {
        using var database = new TestDatabase();
        await using var context = database.CreateContext();
        var movie = await context.Movies.SingleAsync(movie => movie.Title == "Alpha Signal");
        var watchlist = await context.Watchlists.SingleAsync(watchlist => watchlist.UserId == TestDatabase.PrimaryUserId);
        context.WatchlistItems.Add(new WatchlistItem
        {
            MovieId = movie.Id,
            WatchlistId = watchlist.Id,
            Status = WatchlistItemStatus.Planned,
            AddedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var service = new AdminCatalogService(context);
        var result = await service.DeleteMovieAsync(movie.Id);

        Assert.False(result.Succeeded);
        Assert.Contains("cannot be deleted", result.Message);
        Assert.True(await context.Movies.AnyAsync(existing => existing.Id == movie.Id));
    }

    [Fact]
    public async Task DeleteMovieAsync_RemovesMovieWhenNoUserDataExists()
    {
        using var database = new TestDatabase();
        await using var context = database.CreateContext();
        var movie = await context.Movies.SingleAsync(movie => movie.Title == "Beta Harbor");
        var service = new AdminCatalogService(context);

        var result = await service.DeleteMovieAsync(movie.Id);

        Assert.True(result.Succeeded);
        Assert.False(await context.Movies.AnyAsync(existing => existing.Id == movie.Id));
        Assert.False(await context.MovieGenres.AnyAsync(movieGenre => movieGenre.MovieId == movie.Id));
    }

    [Fact]
    public async Task DeleteGenreAsync_BlocksAssignedGenre()
    {
        using var database = new TestDatabase();
        await using var context = database.CreateContext();
        var genre = await context.Genres.SingleAsync(genre => genre.Name == "Action");
        var service = new AdminCatalogService(context);

        var result = await service.DeleteGenreAsync(genre.Id);

        Assert.False(result.Succeeded);
        Assert.Contains("assigned to movies", result.Message);
        Assert.True(await context.Genres.AnyAsync(existing => existing.Id == genre.Id));
    }

    [Fact]
    public async Task AssignAndRemoveGenre_ChangesOnlyMovieGenreRelationship()
    {
        using var database = new TestDatabase();
        await using var context = database.CreateContext();
        var movie = await context.Movies.SingleAsync(movie => movie.Title == "Alpha Signal");
        var genre = await context.Genres.SingleAsync(genre => genre.Name == "Drama");
        var service = new AdminCatalogService(context);

        var assignResult = await service.AssignGenreAsync(movie.Id, genre.Id);
        var removeResult = await service.RemoveGenreFromMovieAsync(movie.Id, genre.Id);

        Assert.True(assignResult.Succeeded);
        Assert.True(removeResult.Succeeded);
        Assert.True(await context.Genres.AnyAsync(existing => existing.Id == genre.Id));
        Assert.False(await context.MovieGenres.AnyAsync(movieGenre =>
            movieGenre.MovieId == movie.Id &&
            movieGenre.GenreId == genre.Id));
    }
}
