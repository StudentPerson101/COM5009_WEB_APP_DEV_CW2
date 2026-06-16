using Microsoft.EntityFrameworkCore;
using MovieWatchlistTracker.Web.Data;
using MovieWatchlistTracker.Web.Models;
using MovieWatchlistTracker.Web.Services.Interfaces;

namespace MovieWatchlistTracker.Web.Services;

public class RatingService : IRatingService
{
    private readonly ApplicationDbContext _context;

    public RatingService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Rating> CreateOrUpdateAsync(string userId, int movieId, double score)
    {
        if (!double.IsFinite(score) || score is < 1 or > 10)
        {
            throw new ArgumentOutOfRangeException(nameof(score), "Rating must be between 1 and 10.");
        }

        score = Math.Round(score, 1, MidpointRounding.AwayFromZero);

        var movieExists = await _context.Movies.AnyAsync(movie => movie.Id == movieId);
        if (!movieExists)
        {
            throw new InvalidOperationException("Movie was not found.");
        }

        var rating = await _context.Ratings
            .SingleOrDefaultAsync(existing => existing.UserId == userId && existing.MovieId == movieId);

        if (rating is null)
        {
            rating = new Rating
            {
                UserId = userId,
                MovieId = movieId,
                Score = score
            };

            _context.Ratings.Add(rating);
        }
        else
        {
            rating.Score = score;
        }

        await _context.SaveChangesAsync();
        return rating;
    }

    public async Task<bool> DeleteAsync(int ratingId, string userId)
    {
        var rating = await _context.Ratings
            .SingleOrDefaultAsync(existing => existing.Id == ratingId && existing.UserId == userId);

        if (rating is null)
        {
            return false;
        }

        _context.Ratings.Remove(rating);
        await _context.SaveChangesAsync();
        return true;
    }

    public Task<bool> UserOwnsRatingAsync(int ratingId, string userId)
    {
        return _context.Ratings
            .AnyAsync(rating => rating.Id == ratingId && rating.UserId == userId);
    }
}
