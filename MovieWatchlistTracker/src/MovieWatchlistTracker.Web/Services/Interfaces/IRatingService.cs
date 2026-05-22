using MovieWatchlistTracker.Web.Models;

namespace MovieWatchlistTracker.Web.Services.Interfaces;

public interface IRatingService
{
    Task<Rating> CreateOrUpdateAsync(string userId, int movieId, int score);
    Task<bool> DeleteAsync(int ratingId, string userId);
    Task<bool> UserOwnsRatingAsync(int ratingId, string userId);
}
