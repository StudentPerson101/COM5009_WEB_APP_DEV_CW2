using Microsoft.EntityFrameworkCore;
using MovieWatchlistTracker.Web.Data;
using MovieWatchlistTracker.Web.Models;
using MovieWatchlistTracker.Web.Services.Interfaces;

namespace MovieWatchlistTracker.Web.Services;

public class ReviewService : IReviewService
{
    private const int MaximumReviewLength = 4000;
    private readonly ApplicationDbContext _context;

    public ReviewService(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<Review?> GetReviewForUserAsync(int reviewId, string userId)
    {
        return _context.Reviews
            .Include(review => review.Movie)
            .SingleOrDefaultAsync(review => review.Id == reviewId && review.UserId == userId);
    }

    public async Task<IReadOnlyList<Review>> GetReviewsForMovieAsync(int movieId)
    {
        return await _context.Reviews
            .AsNoTracking()
            .Include(review => review.User)
            .Where(review => review.MovieId == movieId)
            .OrderByDescending(review => review.UpdatedAt)
            .ToListAsync();
    }

    public Task<bool> ReviewExistsAsync(int reviewId)
    {
        return _context.Reviews.AnyAsync(review => review.Id == reviewId);
    }

    public async Task<Review> CreateOrUpdateForMovieAsync(string userId, int movieId, string text)
    {
        var movieExists = await _context.Movies.AnyAsync(movie => movie.Id == movieId);
        if (!movieExists)
        {
            throw new InvalidOperationException("Movie was not found.");
        }

        var trimmedText = ValidateAndNormalizeText(text);
        var review = await _context.Reviews
            .SingleOrDefaultAsync(existing => existing.UserId == userId && existing.MovieId == movieId);

        if (review is null)
        {
            var now = DateTime.UtcNow;
            review = new Review
            {
                UserId = userId,
                MovieId = movieId,
                Text = trimmedText,
                CreatedAt = now,
                UpdatedAt = now
            };

            _context.Reviews.Add(review);
        }
        else
        {
            review.Text = trimmedText;
            review.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return review;
    }

    public async Task<bool> UpdateAsync(int reviewId, string userId, string text)
    {
        var review = await _context.Reviews
            .SingleOrDefaultAsync(existing => existing.Id == reviewId && existing.UserId == userId);

        if (review is null)
        {
            return false;
        }

        review.Text = ValidateAndNormalizeText(text);
        review.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int reviewId, string userId)
    {
        var review = await _context.Reviews
            .SingleOrDefaultAsync(existing => existing.Id == reviewId && existing.UserId == userId);

        if (review is null)
        {
            return false;
        }

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();
        return true;
    }

    public Task<bool> UserOwnsReviewAsync(int reviewId, string userId)
    {
        return _context.Reviews
            .AnyAsync(review => review.Id == reviewId && review.UserId == userId);
    }

    private static string ValidateAndNormalizeText(string text)
    {
        var trimmedText = text.Trim();

        if (string.IsNullOrWhiteSpace(trimmedText))
        {
            throw new ArgumentException("Review text is required.", nameof(text));
        }

        if (trimmedText.Length > MaximumReviewLength)
        {
            throw new ArgumentException($"Review text must be {MaximumReviewLength} characters or fewer.", nameof(text));
        }

        return trimmedText;
    }
}
