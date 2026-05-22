using MovieWatchlistTracker.Web.Models;
using MovieWatchlistTracker.Web.ViewModels;

namespace MovieWatchlistTracker.Web.Services.Interfaces;

public interface IWatchlistService
{
    Task<Watchlist> EnsureDefaultWatchlistAsync(string userId);
    Task<WatchlistViewModel> GetUserWatchlistAsync(
        string userId,
        string? query = null,
        string? sortBy = null,
        int? genreId = null,
        int? year = null,
        int? minimumRating = null,
        string? status = null);

    Task<IReadOnlyList<MovieTitleSuggestionViewModel>> GetTitleSuggestionsAsync(string userId, string? query, int limit = 8);

    Task<WatchlistItem> AddMovieAsync(string userId, int movieId);
    Task<bool> IsMovieInUserWatchlistAsync(string userId, int movieId);
    Task<bool> RemoveMovieAsync(string userId, int movieId);
    Task<bool> RemoveItemAsync(int watchlistItemId, string userId);
    Task<bool> MarkWatchedAsync(int watchlistItemId, string userId, DateTime? watchedAt = null);
    Task<bool> MarkUnwatchedAsync(int watchlistItemId, string userId);
    Task<bool> UserOwnsWatchlistAsync(int watchlistId, string userId);
    Task<bool> UserOwnsWatchlistItemAsync(int watchlistItemId, string userId);
}
