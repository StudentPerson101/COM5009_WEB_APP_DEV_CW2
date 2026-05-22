using MovieWatchlistTracker.Web.Models;
using MovieWatchlistTracker.Web.ViewModels;

namespace MovieWatchlistTracker.Web.Services.Interfaces;

public interface IViewingHistoryService
{
    Task<HistoryViewModel> GetHistoryAsync(string userId, string? sortBy = null, int? genreId = null);
    Task<ViewingHistoryItem> MarkWatchedAsync(string userId, int movieId, DateTime? watchedAt = null);
    Task<bool> MarkUnwatchedAsync(string userId, int movieId);
    Task<bool> UserOwnsHistoryItemAsync(int historyItemId, string userId);
}
