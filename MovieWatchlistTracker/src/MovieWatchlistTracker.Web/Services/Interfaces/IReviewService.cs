using MovieWatchlistTracker.Web.Models;

namespace MovieWatchlistTracker.Web.Services.Interfaces;

public interface IReviewService
{
    Task<Review?> GetReviewForUserAsync(int reviewId, string userId);
    Task<IReadOnlyList<Review>> GetReviewsForMovieAsync(int movieId);
    Task<bool> ReviewExistsAsync(int reviewId);
    Task<Review> CreateOrUpdateForMovieAsync(string userId, int movieId, string text);
    Task<bool> UpdateAsync(int reviewId, string userId, string text);
    Task<bool> DeleteAsync(int reviewId, string userId);
    Task<bool> UserOwnsReviewAsync(int reviewId, string userId);
}
