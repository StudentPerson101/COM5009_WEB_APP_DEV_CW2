using Microsoft.EntityFrameworkCore;
using MovieWatchlistTracker.Web.Data;
using MovieWatchlistTracker.Web.Models;
using MovieWatchlistTracker.Web.Services.Interfaces;
using MovieWatchlistTracker.Web.ViewModels;

namespace MovieWatchlistTracker.Web.Services;

public class WatchlistService : IWatchlistService
{
    private const string DefaultWatchlistName = "My Watchlist";
    private readonly ApplicationDbContext _context;

    public WatchlistService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Watchlist> EnsureDefaultWatchlistAsync(string userId)
    {
        var watchlist = await _context.Watchlists
            .SingleOrDefaultAsync(existing =>
                existing.UserId == userId &&
                existing.Name == DefaultWatchlistName);

        if (watchlist is not null)
        {
            return watchlist;
        }

        watchlist = new Watchlist
        {
            UserId = userId,
            Name = DefaultWatchlistName,
            CreatedAt = DateTime.UtcNow
        };

        _context.Watchlists.Add(watchlist);
        await _context.SaveChangesAsync();

        return watchlist;
    }

    public async Task<WatchlistViewModel> GetUserWatchlistAsync(
        string userId,
        string? query = null,
        string? sortBy = null,
        int? genreId = null,
        int? year = null,
        int? minimumRating = null,
        string? status = null)
    {
        var items = _context.WatchlistItems
            .AsNoTracking()
            .Include(item => item.Watchlist)
            .Include(item => item.Movie)
                .ThenInclude(movie => movie!.MovieGenres)
                    .ThenInclude(movieGenre => movieGenre.Genre)
            .Include(item => item.Movie)
                .ThenInclude(movie => movie!.Ratings)
            .Where(item => item.Watchlist != null && item.Watchlist.UserId == userId);

        if (!string.IsNullOrWhiteSpace(query))
        {
            var term = query.Trim();
            items = items.Where(item => item.Movie != null && item.Movie.Title.Contains(term));
        }

        if (genreId.HasValue)
        {
            items = items.Where(item =>
                item.Movie != null &&
                item.Movie.MovieGenres.Any(movieGenre => movieGenre.GenreId == genreId));
        }

        if (year.HasValue)
        {
            items = items.Where(item => item.Movie != null && item.Movie.ReleaseYear == year);
        }

        if (minimumRating.HasValue)
        {
            items = items.Where(item =>
                item.Movie != null &&
                item.Movie.Ratings.Any() &&
                item.Movie.Ratings.Average(rating => rating.Score) >= minimumRating.Value);
        }

        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<WatchlistItemStatus>(status, ignoreCase: true, out var parsedStatus))
        {
            items = items.Where(item => item.Status == parsedStatus);
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
            "status" => items
                .OrderBy(item => item.Status)
                .ThenBy(item => item.Movie!.Title),
            _ => items
                .OrderByDescending(item => item.AddedAt)
                .ThenBy(item => item.Movie!.Title)
        };

        return new WatchlistViewModel
        {
            Query = query,
            SortBy = sortBy,
            SelectedGenreId = genreId,
            SelectedYear = year,
            SelectedRating = minimumRating,
            SelectedStatus = status,
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
                    WatchlistItemId = item.Id,
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
                    Status = item.Status.ToString().ToLowerInvariant(),
                    AddedAt = item.AddedAt
                })
                .ToListAsync()
        };
    }

    public async Task<IReadOnlyList<MovieTitleSuggestionViewModel>> GetTitleSuggestionsAsync(
        string userId,
        string? query,
        int limit = 8)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Trim().Length < 2 || limit <= 0)
        {
            return [];
        }

        var term = query.Trim();
        var escapedTerm = EscapeLikePattern(term);
        var cappedLimit = Math.Min(limit, 12);

        return await _context.WatchlistItems
            .AsNoTracking()
            .Where(item =>
                item.Watchlist != null &&
                item.Watchlist.UserId == userId &&
                item.Movie != null &&
                EF.Functions.Like(item.Movie.Title, $"%{escapedTerm}%", @"\"))
            .OrderBy(item => item.Movie!.Title)
            .ThenByDescending(item => item.Movie!.ReleaseYear)
            .Take(cappedLimit)
            .Select(item => new MovieTitleSuggestionViewModel
            {
                MovieId = item.MovieId,
                Title = item.Movie!.Title,
                ReleaseYear = item.Movie.ReleaseYear
            })
            .ToListAsync();
    }

    public async Task<WatchlistItem> AddMovieAsync(string userId, int movieId)
    {
        var movieExists = await _context.Movies.AnyAsync(movie => movie.Id == movieId);
        if (!movieExists)
        {
            throw new InvalidOperationException("Movie was not found.");
        }

        var watchlist = await EnsureDefaultWatchlistAsync(userId);
        var existingItem = await _context.WatchlistItems
            .SingleOrDefaultAsync(item => item.WatchlistId == watchlist.Id && item.MovieId == movieId);

        if (existingItem is not null)
        {
            return existingItem;
        }

        var item = new WatchlistItem
        {
            WatchlistId = watchlist.Id,
            MovieId = movieId,
            Status = WatchlistItemStatus.Planned,
            AddedAt = DateTime.UtcNow
        };

        _context.WatchlistItems.Add(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public Task<bool> IsMovieInUserWatchlistAsync(string userId, int movieId)
    {
        return _context.WatchlistItems
            .AnyAsync(item =>
                item.MovieId == movieId &&
                item.Watchlist != null &&
                item.Watchlist.UserId == userId);
    }

    public async Task<bool> RemoveMovieAsync(string userId, int movieId)
    {
        var item = await _context.WatchlistItems
            .Include(existing => existing.Watchlist)
            .SingleOrDefaultAsync(existing =>
                existing.MovieId == movieId &&
                existing.Watchlist != null &&
                existing.Watchlist.UserId == userId);

        if (item is null)
        {
            return false;
        }

        _context.WatchlistItems.Remove(item);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveItemAsync(int watchlistItemId, string userId)
    {
        var item = await _context.WatchlistItems
            .Include(existing => existing.Watchlist)
            .SingleOrDefaultAsync(existing =>
                existing.Id == watchlistItemId &&
                existing.Watchlist != null &&
                existing.Watchlist.UserId == userId);

        if (item is null)
        {
            return false;
        }

        _context.WatchlistItems.Remove(item);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> MarkWatchedAsync(int watchlistItemId, string userId, DateTime? watchedAt = null)
    {
        var item = await _context.WatchlistItems
            .Include(existing => existing.Watchlist)
            .SingleOrDefaultAsync(existing =>
                existing.Id == watchlistItemId &&
                existing.Watchlist != null &&
                existing.Watchlist.UserId == userId);

        if (item is null)
        {
            return false;
        }

        item.Status = WatchlistItemStatus.Watched;
        var historyItem = await _context.ViewingHistoryItems
            .SingleOrDefaultAsync(existing => existing.UserId == userId && existing.MovieId == item.MovieId);

        if (historyItem is null)
        {
            _context.ViewingHistoryItems.Add(new ViewingHistoryItem
            {
                UserId = userId,
                MovieId = item.MovieId,
                WatchedAt = watchedAt ?? DateTime.UtcNow
            });
        }
        else
        {
            historyItem.WatchedAt = watchedAt ?? DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> MarkUnwatchedAsync(int watchlistItemId, string userId)
    {
        var item = await _context.WatchlistItems
            .Include(existing => existing.Watchlist)
            .SingleOrDefaultAsync(existing =>
                existing.Id == watchlistItemId &&
                existing.Watchlist != null &&
                existing.Watchlist.UserId == userId);

        if (item is null)
        {
            return false;
        }

        item.Status = WatchlistItemStatus.Planned;

        var historyItems = await _context.ViewingHistoryItems
            .Where(existing => existing.UserId == userId && existing.MovieId == item.MovieId)
            .ToListAsync();

        _context.ViewingHistoryItems.RemoveRange(historyItems);
        await _context.SaveChangesAsync();
        return true;
    }

    public Task<bool> UserOwnsWatchlistAsync(int watchlistId, string userId)
    {
        return _context.Watchlists
            .AnyAsync(watchlist => watchlist.Id == watchlistId && watchlist.UserId == userId);
    }

    public Task<bool> UserOwnsWatchlistItemAsync(int watchlistItemId, string userId)
    {
        return _context.WatchlistItems
            .AnyAsync(item =>
                item.Id == watchlistItemId &&
                item.Watchlist != null &&
                item.Watchlist.UserId == userId);
    }

    private static string EscapeLikePattern(string value)
    {
        return value
            .Replace(@"\", @"\\")
            .Replace("%", @"\%")
            .Replace("_", @"\_");
    }
}
