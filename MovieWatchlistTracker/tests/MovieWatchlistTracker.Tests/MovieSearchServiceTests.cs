using Microsoft.EntityFrameworkCore;
using MovieWatchlistTracker.Tests.TestData;
using MovieWatchlistTracker.Web.Models;
using MovieWatchlistTracker.Web.Services;

namespace MovieWatchlistTracker.Tests;

public class MovieSearchServiceTests
{
    [Fact]
    public async Task SearchReturnsMoviesMatchingTitleAndGenre()
    {
        using var database = new TestDatabase();
        await using var context = database.CreateContext();
        var actionGenreId = await context.Genres
            .Where(genre => genre.Name == "Action")
            .Select(genre => genre.Id)
            .SingleAsync();

        var service = new MovieSearchService(context);

        var result = await service.SearchAsync("Alpha", actionGenreId, null, null, null, "title");

        var movie = Assert.Single(result.Movies);
        Assert.Equal("Alpha Signal", movie.Title);
        Assert.Contains("Action", movie.Genres);
    }

    [Fact]
    public async Task DetailsReturnsMovieFieldsAndCurrentUserState()
    {
        using var database = new TestDatabase();
        await using var context = database.CreateContext();
        var movieId = await context.Movies
            .Where(movie => movie.Title == "Alpha Signal")
            .Select(movie => movie.Id)
            .SingleAsync();

        var watchlistService = new WatchlistService(context);
        var watchlistItem = await watchlistService.AddMovieAsync(TestDatabase.PrimaryUserId, movieId);
        await watchlistService.MarkWatchedAsync(watchlistItem.Id, TestDatabase.PrimaryUserId);

        var ratingService = new RatingService(context);
        await ratingService.CreateOrUpdateAsync(TestDatabase.PrimaryUserId, movieId, 5);

        var service = new MovieSearchService(context);

        var details = await service.GetDetailsAsync(movieId, TestDatabase.PrimaryUserId);

        Assert.NotNull(details);
        Assert.Equal("Alpha Signal", details.Title);
        Assert.True(details.IsInWatchlist);
        Assert.True(details.IsWatched);
        Assert.Equal(5, details.CurrentUserRating);
        Assert.Contains("Action", details.Genres);
    }

    [Fact]
    public async Task TitleSuggestionsUseCurrentMoviesTableData()
    {
        using var database = new TestDatabase();
        await using var context = database.CreateContext();

        context.Movies.Add(new Movie
        {
            Title = "Gamma Admin Addition",
            ReleaseYear = 2026,
            Description = "A movie added after the initial catalog seed.",
            Runtime = 112
        });
        await context.SaveChangesAsync();

        var service = new MovieSearchService(context);

        var suggestions = await service.GetTitleSuggestionsAsync("Gamma");

        var suggestion = Assert.Single(suggestions);
        Assert.Equal("Gamma Admin Addition", suggestion.Title);
        Assert.Equal(2026, suggestion.ReleaseYear);
    }

    [Fact]
    public async Task TitleSuggestionsIgnoreSingleCharacterQueries()
    {
        using var database = new TestDatabase();
        await using var context = database.CreateContext();
        var service = new MovieSearchService(context);

        var suggestions = await service.GetTitleSuggestionsAsync("A");

        Assert.Empty(suggestions);
    }
}
