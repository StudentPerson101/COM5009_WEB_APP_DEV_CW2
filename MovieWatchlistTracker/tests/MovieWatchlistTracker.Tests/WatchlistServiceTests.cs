using Microsoft.EntityFrameworkCore;
using MovieWatchlistTracker.Tests.TestData;
using MovieWatchlistTracker.Web.Models;
using MovieWatchlistTracker.Web.Services;

namespace MovieWatchlistTracker.Tests;

public class WatchlistServiceTests
{
    [Fact]
    public async Task AddMoviePreventsDuplicateWatchlistItems()
    {
        using var database = new TestDatabase();
        await using var context = database.CreateContext();
        var movieId = await FirstMovieIdAsync(context);
        var service = new WatchlistService(context);

        var first = await service.AddMovieAsync(TestDatabase.PrimaryUserId, movieId);
        var second = await service.AddMovieAsync(TestDatabase.PrimaryUserId, movieId);

        Assert.Equal(first.Id, second.Id);
        Assert.True(await service.IsMovieInUserWatchlistAsync(TestDatabase.PrimaryUserId, movieId));
        Assert.Equal(1, await context.WatchlistItems.CountAsync(item => item.MovieId == movieId));
    }

    [Fact]
    public async Task RemoveMovieChecksOwnership()
    {
        using var database = new TestDatabase();
        await using var context = database.CreateContext();
        var movieId = await FirstMovieIdAsync(context);
        var service = new WatchlistService(context);
        await service.AddMovieAsync(TestDatabase.PrimaryUserId, movieId);

        var removedByOtherUser = await service.RemoveMovieAsync(TestDatabase.OtherUserId, movieId);
        var removedByOwner = await service.RemoveMovieAsync(TestDatabase.PrimaryUserId, movieId);

        Assert.False(removedByOtherUser);
        Assert.True(removedByOwner);
        Assert.Empty(await context.WatchlistItems.Where(item => item.MovieId == movieId).ToListAsync());
    }

    [Fact]
    public async Task MarkWatchedAndUnwatchedSynchronizesHistory()
    {
        using var database = new TestDatabase();
        await using var context = database.CreateContext();
        var movieId = await FirstMovieIdAsync(context);
        var service = new WatchlistService(context);
        var item = await service.AddMovieAsync(TestDatabase.PrimaryUserId, movieId);

        var watched = await service.MarkWatchedAsync(item.Id, TestDatabase.PrimaryUserId, new DateTime(2026, 1, 2));
        var watchedItem = await context.WatchlistItems.SingleAsync(existing => existing.Id == item.Id);
        var historyItem = await context.ViewingHistoryItems.SingleAsync(existing =>
            existing.UserId == TestDatabase.PrimaryUserId &&
            existing.MovieId == movieId);

        Assert.True(watched);
        Assert.Equal(WatchlistItemStatus.Watched, watchedItem.Status);
        Assert.Equal(new DateTime(2026, 1, 2), historyItem.WatchedAt);

        var unwatched = await service.MarkUnwatchedAsync(item.Id, TestDatabase.PrimaryUserId);
        var unwatchedItem = await context.WatchlistItems.SingleAsync(existing => existing.Id == item.Id);

        Assert.True(unwatched);
        Assert.Equal(WatchlistItemStatus.Planned, unwatchedItem.Status);
        Assert.Empty(await context.ViewingHistoryItems.Where(existing => existing.MovieId == movieId).ToListAsync());
    }

    [Fact]
    public async Task TitleSuggestionsOnlyIncludeCurrentUsersWatchlistMovies()
    {
        using var database = new TestDatabase();
        await using var context = database.CreateContext();
        var firstMovieId = await FirstMovieIdAsync(context);
        var otherMovieId = await context.Movies
            .Where(movie => movie.Title == "Beta Harbor")
            .Select(movie => movie.Id)
            .SingleAsync();

        var service = new WatchlistService(context);
        await service.AddMovieAsync(TestDatabase.PrimaryUserId, firstMovieId);
        await service.AddMovieAsync(TestDatabase.OtherUserId, otherMovieId);

        var suggestions = await service.GetTitleSuggestionsAsync(TestDatabase.PrimaryUserId, "a");
        Assert.Empty(suggestions);

        suggestions = await service.GetTitleSuggestionsAsync(TestDatabase.PrimaryUserId, "ha");

        var suggestion = Assert.Single(suggestions);
        Assert.Equal("Alpha Signal", suggestion.Title);
    }

    private static Task<int> FirstMovieIdAsync(DbContext context)
    {
        return context.Set<Movie>()
            .Where(movie => movie.Title == "Alpha Signal")
            .Select(movie => movie.Id)
            .SingleAsync();
    }
}
