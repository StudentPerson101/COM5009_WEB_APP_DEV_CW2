using Microsoft.EntityFrameworkCore;
using MovieWatchlistTracker.Tests.TestData;
using MovieWatchlistTracker.Web.Models;
using MovieWatchlistTracker.Web.Services;

namespace MovieWatchlistTracker.Tests;

public class ReviewServiceTests
{
    [Fact]
    public async Task CreateOrUpdateStoresSingleUserMovieReview()
    {
        using var database = new TestDatabase();
        await using var context = database.CreateContext();
        var movieId = await MovieIdAsync(context);
        var service = new ReviewService(context);

        var created = await service.CreateOrUpdateForMovieAsync(TestDatabase.PrimaryUserId, movieId, " First take ");
        var updated = await service.CreateOrUpdateForMovieAsync(TestDatabase.PrimaryUserId, movieId, "Second take");

        Assert.Equal(created.Id, updated.Id);
        Assert.Equal("Second take", updated.Text);
        Assert.Equal(1, await context.Reviews.CountAsync(review => review.MovieId == movieId));
    }

    [Fact]
    public async Task ReviewTextIsRequired()
    {
        using var database = new TestDatabase();
        await using var context = database.CreateContext();
        var movieId = await MovieIdAsync(context);
        var service = new ReviewService(context);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateOrUpdateForMovieAsync(TestDatabase.PrimaryUserId, movieId, "   "));
    }

    [Fact]
    public async Task ReviewTextHasMaximumLength()
    {
        using var database = new TestDatabase();
        await using var context = database.CreateContext();
        var movieId = await MovieIdAsync(context);
        var service = new ReviewService(context);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateOrUpdateForMovieAsync(TestDatabase.PrimaryUserId, movieId, new string('x', 4001)));
    }

    [Fact]
    public async Task UpdateAndDeleteCheckOwnership()
    {
        using var database = new TestDatabase();
        await using var context = database.CreateContext();
        var movieId = await MovieIdAsync(context);
        var service = new ReviewService(context);
        var review = await service.CreateOrUpdateForMovieAsync(TestDatabase.PrimaryUserId, movieId, "Original");

        var updatedByOtherUser = await service.UpdateAsync(review.Id, TestDatabase.OtherUserId, "Other edit");
        var updatedByOwner = await service.UpdateAsync(review.Id, TestDatabase.PrimaryUserId, "Owner edit");
        var deletedByOtherUser = await service.DeleteAsync(review.Id, TestDatabase.OtherUserId);
        var deletedByOwner = await service.DeleteAsync(review.Id, TestDatabase.PrimaryUserId);

        Assert.False(updatedByOtherUser);
        Assert.True(updatedByOwner);
        Assert.False(deletedByOtherUser);
        Assert.True(deletedByOwner);
        Assert.Empty(await context.Reviews.ToListAsync());
    }

    private static Task<int> MovieIdAsync(DbContext context)
    {
        return context.Set<Movie>()
            .Where(movie => movie.Title == "Alpha Signal")
            .Select(movie => movie.Id)
            .SingleAsync();
    }
}
