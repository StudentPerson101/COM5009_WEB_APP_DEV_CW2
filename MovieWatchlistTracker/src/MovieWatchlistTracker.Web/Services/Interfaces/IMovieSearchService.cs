using MovieWatchlistTracker.Web.ViewModels;

namespace MovieWatchlistTracker.Web.Services.Interfaces;

public interface IMovieSearchService
{
    Task<MovieSearchViewModel> SearchAsync(
        string? query,
        int? genreId,
        int? year,
        int? minimumRating,
        string? watchedStatus,
        string? sortBy,
        string? userId = null);

    Task<IReadOnlyList<MovieTitleSuggestionViewModel>> GetTitleSuggestionsAsync(string? query, int limit = 8);

    Task<MovieDetailsViewModel?> GetDetailsAsync(int movieId, string? userId = null);
}
