using Microsoft.EntityFrameworkCore;
using MovieWatchlistTracker.Web.Data;
using MovieWatchlistTracker.Web.Models;
using MovieWatchlistTracker.Web.Services.Interfaces;
using MovieWatchlistTracker.Web.ViewModels;

namespace MovieWatchlistTracker.Web.Services;

public class ViewingHistoryService : IViewingHistoryService
{
    private readonly ApplicationDbContext _context;

    public ViewingHistoryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<HistoryViewModel> GetHistoryAsync(string userId, string? sortBy = null, int? genreId = null)
    {
        var items = _context.ViewingHistoryItems
            .AsNoTracking()
            .Include(item => item.Movie)
                .ThenInclude(movie => movie!.MovieGenres)
                    .ThenInclude(movieGenre => movieGenre.Genre)
            .Include(item => item.Movie)
                .ThenInclude(movie => movie!.Ratings)
            .Where(item => item.UserId == userId);

        if (genreId.HasValue)
        {
            items = items.Where(item =>
                item.Movie != null &&
                item.Movie.MovieGenres.Any(movieGenre => movieGenre.GenreId == genreId));
        }

        items = (sortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "title" => items.OrderBy(item => item.Movie!.Title),
            "releaseyear" or "release-year" or "year" => items
                .OrderByDescending(item => item.Movie!.ReleaseYear)
                .ThenBy(item => item.Movie!.Title),
            "rating" => items
                .OrderByDescending(item => item.Movie!.Ratings.Any()
                    ? item.Movie.Ratings.Average(rating => rating.Score)
                    : 0)
                .ThenBy(item => item.Movie!.Title),
            _ => items
                .OrderByDescending(item => item.WatchedAt)
                .ThenBy(item => item.Movie!.Title)
        };

        return new HistoryViewModel
        {
            SortBy = sortBy,
            SelectedGenreId = genreId,
            Genres = await _context.Genres
                .AsNoTracking()
                .OrderBy(genre => genre.Name)
                .Select(genre => new GenreFilterOptionViewModel
                {
                    Id = genre.Id,
                    Name = genre.Name
                })
                .ToListAsync(),
            Items = await items
                .Select(item => new WatchlistItemViewModel
                {
                    MovieId = item.MovieId,
                    Title = item.Movie!.Title,
                    ReleaseYear = item.Movie.ReleaseYear,
                    PosterUrl = item.Movie.PosterUrl,
                    Genres = item.Movie.MovieGenres
                        .Select(movieGenre => movieGenre.Genre!.Name)
                        .OrderBy(name => name)
                        .ToList(),
                    AverageRating = item.Movie.Ratings.Count == 0
                        ? null
                        : Math.Round(item.Movie.Ratings.Average(rating => rating.Score), 1),
                    Status = WatchlistItemStatus.Watched.ToString().ToLowerInvariant(),
                    AddedAt = item.WatchedAt,
                    WatchedAt = item.WatchedAt
                })
                .ToListAsync()
        };
    }

    public async Task<ViewingHistoryItem> MarkWatchedAsync(string userId, int movieId, DateTime? watchedAt = null)
    {
        var movieExists = await _context.Movies.AnyAsync(movie => movie.Id == movieId);
        if (!movieExists)
        {
            throw new InvalidOperationException("Movie was not found.");
        }

        var watchedTimestamp = watchedAt ?? DateTime.UtcNow;
        var historyItem = await _context.ViewingHistoryItems
            .SingleOrDefaultAsync(existing => existing.UserId == userId && existing.MovieId == movieId);

        if (historyItem is null)
        {
            historyItem = new ViewingHistoryItem
            {
                UserId = userId,
                MovieId = movieId,
                WatchedAt = watchedTimestamp
            };

            _context.ViewingHistoryItems.Add(historyItem);
        }
        else
        {
            historyItem.WatchedAt = watchedTimestamp;
        }

        var watchlistItem = await _context.WatchlistItems
            .Include(item => item.Watchlist)
            .SingleOrDefaultAsync(item =>
                item.MovieId == movieId &&
                item.Watchlist != null &&
                item.Watchlist.UserId == userId);

        if (watchlistItem is not null)
        {
            watchlistItem.Status = WatchlistItemStatus.Watched;
        }

        await _context.SaveChangesAsync();
        return historyItem;
    }

    public async Task<bool> MarkUnwatchedAsync(string userId, int movieId)
    {
        var historyItems = await _context.ViewingHistoryItems
            .Where(existing => existing.UserId == userId && existing.MovieId == movieId)
            .ToListAsync();

        var watchlistItem = await _context.WatchlistItems
            .Include(item => item.Watchlist)
            .SingleOrDefaultAsync(item =>
                item.MovieId == movieId &&
                item.Watchlist != null &&
                item.Watchlist.UserId == userId);

        if (watchlistItem is not null)
        {
            watchlistItem.Status = WatchlistItemStatus.Planned;
        }

        if (historyItems.Count == 0 && watchlistItem is null)
        {
            return false;
        }

        _context.ViewingHistoryItems.RemoveRange(historyItems);
        await _context.SaveChangesAsync();
        return true;
    }

    public Task<bool> UserOwnsHistoryItemAsync(int historyItemId, string userId)
    {
        return _context.ViewingHistoryItems
            .AnyAsync(item => item.Id == historyItemId && item.UserId == userId);
    }
}
