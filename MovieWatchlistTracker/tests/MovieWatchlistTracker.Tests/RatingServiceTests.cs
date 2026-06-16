using Microsoft.EntityFrameworkCore;
using MovieWatchlistTracker.Tests.TestData;
using MovieWatchlistTracker.Web.Models;
using MovieWatchlistTracker.Web.Services;

namespace MovieWatchlistTracker.Tests;

public class RatingServiceTests
{
    [Fact]
    public async Task CreateOrUpdateStoresSingleUserMovieRating()
    {
        using var database = new TestDatabase();
        await using var context = database.CreateContext();
        var movieId = await MovieIdAsync(context);
        var service = new RatingService(context);

        var created = await service.CreateOrUpdateAsync(TestDatabase.PrimaryUserId, movieId, 7.4);
        var updated = await service.CreateOrUpdateAsync(TestDatabase.PrimaryUserId, movieId, 8.25);

        Assert.Equal(created.Id, updated.Id);
        Assert.Equal(8.3, updated.Score);
        Assert.Equal(1, await context.Ratings.CountAsync(rating => rating.MovieId == movieId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10.1)]
    public async Task CreateOrUpdateRejectsInvalidRating(double score)
    {
        using var database = new TestDatabase();
        await using var context = database.CreateContext();
        var movieId = await MovieIdAsync(context);
        var service = new RatingService(context);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            service.CreateOrUpdateAsync(TestDatabase.PrimaryUserId, movieId, score));

        Assert.Empty(await context.Ratings.ToListAsync());
    }

    [Fact]
    public async Task DeleteChecksOwnership()
    {
        using var database = new TestDatabase();
        await using var context = database.CreateContext();
        var movieId = await MovieIdAsync(context);
        var service = new RatingService(context);
        var rating = await service.CreateOrUpdateAsync(TestDatabase.PrimaryUserId, movieId, 8.0);

        var deletedByOtherUser = await service.DeleteAsync(rating.Id, TestDatabase.OtherUserId);
        var deletedByOwner = await service.DeleteAsync(rating.Id, TestDatabase.PrimaryUserId);

        Assert.False(deletedByOtherUser);
        Assert.True(deletedByOwner);
        Assert.Empty(await context.Ratings.ToListAsync());
    }

    private static Task<int> MovieIdAsync(DbContext context)
    {
        return context.Set<Movie>()
            .Where(movie => movie.Title == "Alpha Signal")
            .Select(movie => movie.Id)
            .SingleAsync();
    }
}
