using Microsoft.EntityFrameworkCore;
using MovieWatchlistTracker.Web.Data;
using MovieWatchlistTracker.Web.Models;
using MovieWatchlistTracker.Web.Services.Interfaces;
using MovieWatchlistTracker.Web.ViewModels;

namespace MovieWatchlistTracker.Web.Services;

public class MovieSearchService : IMovieSearchService
{
    private readonly ApplicationDbContext _context;

    public MovieSearchService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MovieSearchViewModel> SearchAsync(
        string? query,
        int? genreId,
        int? year,
        int? minimumRating,
        string? watchedStatus,
        string? sortBy,
        string? userId = null)
    {
        var movies = ApplyMovieFilters(
            BaseMovieQuery(),
            query,
            genreId,
            year,
            minimumRating,
            watchedStatus,
            sortBy,
            userId);

        return new MovieSearchViewModel
        {
            Query = query,
            SelectedGenreId = genreId,
            SelectedYear = year,
            SelectedRating = minimumRating,
            SelectedWatchedStatus = watchedStatus,
            SortBy = sortBy,
            Genres = await _context.Genres
                .AsNoTracking()
                .OrderBy(genre => genre.Name)
                .Select(genre => new GenreFilterOptionViewModel
                {
                    Id = genre.Id,
                    Name = genre.Name
                })
                .ToListAsync(),
            Movies = await movies
                .Select(movie => new MovieCardViewModel
                {
                    MovieId = movie.Id,
                    Title = movie.Title,
                    ReleaseYear = movie.ReleaseYear,
                    PosterUrl = movie.PosterUrl,
                    Runtime = movie.Runtime,
                    Genres = movie.MovieGenres
                        .Select(movieGenre => movieGenre.Genre!.Name)
                        .OrderBy(name => name)
                        .ToList(),
                    AverageRating = movie.Ratings.Count == 0
                        ? null
                        : Math.Round((decimal)movie.Ratings.Average(rating => rating.Score), 1),
                    IsInCurrentUserWatchlist = userId != null &&
                        movie.WatchlistItems.Any(item => item.Watchlist != null && item.Watchlist.UserId == userId)
                })
                .ToListAsync()
        };
    }

    public async Task<MovieDetailsViewModel?> GetDetailsAsync(int movieId, string? userId = null)
    {
        var movie = await BaseMovieQuery()
            .Where(movie => movie.Id == movieId)
            .SingleOrDefaultAsync();

        if (movie is null)
        {
            return null;
        }

        var currentUserRating = userId == null
            ? null
            : movie.Ratings.SingleOrDefault(rating => rating.UserId == userId);
        var currentUserReview = userId == null
            ? null
            : movie.Reviews.SingleOrDefault(review => review.UserId == userId);

        return new MovieDetailsViewModel
        {
            MovieId = movie.Id,
            Title = movie.Title,
            ReleaseYear = movie.ReleaseYear,
            Description = movie.Description,
            PosterUrl = movie.PosterUrl,
            Runtime = movie.Runtime,
            Genres = movie.MovieGenres
                .Select(movieGenre => movieGenre.Genre!.Name)
                .OrderBy(name => name)
                .ToList(),
            AverageRating = movie.Ratings.Count == 0
                ? null
                : Math.Round((decimal)movie.Ratings.Average(rating => rating.Score), 1),
            CurrentUserRatingId = currentUserRating?.Id,
            CurrentUserRating = currentUserRating?.Score,
            CurrentUserReviewId = currentUserReview?.Id,
            CurrentUserReview = currentUserReview?.Text,
            IsInWatchlist = userId != null &&
                movie.WatchlistItems.Any(item => item.Watchlist != null && item.Watchlist.UserId == userId),
            IsWatched = userId != null &&
                movie.ViewingHistoryItems.Any(item => item.UserId == userId),
            OtherReviews = movie.Reviews
                .OrderByDescending(review => review.UpdatedAt)
                .Select(review => new ReviewDisplayViewModel
                {
                    Id = review.Id,
                    ReviewerName = review.User == null
                        ? "Movie watcher"
                        : review.User.UserName ?? "Movie watcher",
                    Rating = movie.Ratings
                        .Where(rating => rating.UserId == review.UserId)
                        .Select(rating => (int?)rating.Score)
                        .SingleOrDefault(),
                    Text = review.Text,
                    CreatedAt = review.CreatedAt,
                    UpdatedAt = review.UpdatedAt,
                    IsCurrentUserReview = userId != null && review.UserId == userId
                })
                .ToList()
        };
    }

    public async Task<IReadOnlyList<MovieTitleSuggestionViewModel>> GetTitleSuggestionsAsync(string? query, int limit = 8)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Trim().Length < 2 || limit <= 0)
        {
            return [];
        }

        var term = query.Trim();
        var escapedTerm = EscapeLikePattern(term);
        var cappedLimit = Math.Min(limit, 12);

        return await _context.Movies
            .AsNoTracking()
            .Where(movie => EF.Functions.Like(movie.Title, $"%{escapedTerm}%", @"\"))
            .OrderBy(movie => movie.Title)
            .ThenByDescending(movie => movie.ReleaseYear)
            .Take(cappedLimit)
            .Select(movie => new MovieTitleSuggestionViewModel
            {
                MovieId = movie.Id,
                Title = movie.Title,
                ReleaseYear = movie.ReleaseYear
            })
            .ToListAsync();
    }

    internal static IQueryable<Movie> ApplyMovieFilters(
        IQueryable<Movie> movies,
        string? query,
        int? genreId,
        int? year,
        int? minimumRating,
        string? watchedStatus,
        string? sortBy,
        string? userId)
    {
        if (!string.IsNullOrWhiteSpace(query))
        {
            var term = query.Trim();
            movies = movies.Where(movie => movie.Title.Contains(term));
        }

        if (genreId.HasValue)
        {
            movies = movies.Where(movie => movie.MovieGenres.Any(movieGenre => movieGenre.GenreId == genreId));
        }

        if (year.HasValue)
        {
            movies = movies.Where(movie => movie.ReleaseYear == year);
        }

        if (minimumRating.HasValue)
        {
            movies = movies.Where(movie =>
                movie.Ratings.Any() &&
                movie.Ratings.Average(rating => rating.Score) >= minimumRating.Value);
        }

        if (!string.IsNullOrWhiteSpace(watchedStatus) && userId is not null)
        {
            var normalizedStatus = watchedStatus.Trim().ToLowerInvariant();
            movies = normalizedStatus switch
            {
                "watched" => movies.Where(movie => movie.ViewingHistoryItems.Any(item => item.UserId == userId)),
                "unwatched" => movies.Where(movie => !movie.ViewingHistoryItems.Any(item => item.UserId == userId)),
                _ => movies
            };
        }

        return (sortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "title" => movies.OrderBy(movie => movie.Title),
            "releaseyear" or "release-year" or "year" => movies
                .OrderByDescending(movie => movie.ReleaseYear)
                .ThenBy(movie => movie.Title),
            "rating" => movies
                .OrderByDescending(movie => movie.Ratings.Any()
                    ? movie.Ratings.Average(rating => rating.Score)
                    : 0)
                .ThenBy(movie => movie.Title),
            _ => movies
                .OrderBy(movie => movie.Title)
                .ThenByDescending(movie => movie.ReleaseYear)
        };
    }

    private IQueryable<Movie> BaseMovieQuery()
    {
        return _context.Movies
            .AsNoTracking()
            .Include(movie => movie.MovieGenres)
                .ThenInclude(movieGenre => movieGenre.Genre)
            .Include(movie => movie.Ratings)
            .Include(movie => movie.Reviews)
                .ThenInclude(review => review.User)
            .Include(movie => movie.ViewingHistoryItems)
            .Include(movie => movie.WatchlistItems)
                .ThenInclude(item => item.Watchlist);
    }

    private static string EscapeLikePattern(string value)
    {
        return value
            .Replace(@"\", @"\\")
            .Replace("%", @"\%")
            .Replace("_", @"\_");
    }
}
