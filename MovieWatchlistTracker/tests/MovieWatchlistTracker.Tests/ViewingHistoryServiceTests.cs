using Microsoft.EntityFrameworkCore;
using MovieWatchlistTracker.Tests.TestData;
using MovieWatchlistTracker.Web.Models;
using MovieWatchlistTracker.Web.Services;

namespace MovieWatchlistTracker.Tests;

public class ViewingHistoryServiceTests
{
    [Fact]
    public async Task MarkWatchedCreatesOrUpdatesHistoryRecord()
    {
        using var database = new TestDatabase();
        await using var context = database.CreateContext();
        var movieId = await MovieIdAsync(context);
        var service = new ViewingHistoryService(context);

        var first = await service.MarkWatchedAsync(TestDatabase.PrimaryUserId, movieId, new DateTime(2026, 2, 1));
        var second = await service.MarkWatchedAsync(TestDatabase.PrimaryUserId, movieId, new DateTime(2026, 2, 3));

        Assert.Equal(first.Id, second.Id);
        Assert.Equal(1, await context.ViewingHistoryItems.CountAsync(item => item.MovieId == movieId));
        Assert.Equal(new DateTime(2026, 2, 3), second.WatchedAt);
    }

    [Fact]
    public async Task MarkUnwatchedUpdatesWatchlistStatusAndRemovesHistory()
    {
        using var database = new TestDatabase();
        await using var context = database.CreateContext();
        var movieId = await MovieIdAsync(context);

        var watchlistService = new WatchlistService(context);
        var watchlistItem = await watchlistService.AddMovieAsync(TestDatabase.PrimaryUserId, movieId);

        var service = new ViewingHistoryService(context);
        await service.MarkWatchedAsync(TestDatabase.PrimaryUserId, movieId);

        var removed = await service.MarkUnwatchedAsync(TestDatabase.PrimaryUserId, movieId);
        var updatedWatchlistItem = await context.WatchlistItems.SingleAsync(item => item.Id == watchlistItem.Id);

        Assert.True(removed);
        Assert.Equal(WatchlistItemStatus.Planned, updatedWatchlistItem.Status);
        Assert.Empty(await context.ViewingHistoryItems.Where(item => item.MovieId == movieId).ToListAsync());
    }

    private static Task<int> MovieIdAsync(DbContext context)
    {
        return context.Set<Movie>()
            .Where(movie => movie.Title == "Alpha Signal")
            .Select(movie => movie.Id)
            .SingleAsync();
    }
}
