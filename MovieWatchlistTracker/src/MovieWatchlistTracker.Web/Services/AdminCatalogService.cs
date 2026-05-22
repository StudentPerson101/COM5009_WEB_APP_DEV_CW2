using Microsoft.EntityFrameworkCore;
using MovieWatchlistTracker.Web.Data;
using MovieWatchlistTracker.Web.Models;
using MovieWatchlistTracker.Web.Services.Interfaces;
using MovieWatchlistTracker.Web.ViewModels;

namespace MovieWatchlistTracker.Web.Services;

public class AdminCatalogService : IAdminCatalogService
{
    private readonly ApplicationDbContext _context;

    public AdminCatalogService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AdminMovieIndexViewModel> GetMovieIndexAsync(string? query, int? genreId, string? sortBy)
    {
        var moviesQuery = _context.Movies
            .AsNoTracking()
            .Include(movie => movie.MovieGenres)
                .ThenInclude(movieGenre => movieGenre.Genre)
            .Include(movie => movie.WatchlistItems)
            .Include(movie => movie.ViewingHistoryItems)
            .Include(movie => movie.Ratings)
            .Include(movie => movie.Reviews)
            .AsSplitQuery()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var trimmedQuery = query.Trim();
            moviesQuery = moviesQuery.Where(movie => movie.Title.Contains(trimmedQuery));
        }

        if (genreId.HasValue)
        {
            moviesQuery = moviesQuery.Where(movie =>
                movie.MovieGenres.Any(movieGenre => movieGenre.GenreId == genreId.Value));
        }

        moviesQuery = sortBy switch
        {
            "year" => moviesQuery.OrderByDescending(movie => movie.ReleaseYear).ThenBy(movie => movie.Title),
            "runtime" => moviesQuery.OrderBy(movie => movie.Runtime ?? int.MaxValue).ThenBy(movie => movie.Title),
            _ => moviesQuery.OrderBy(movie => movie.Title).ThenByDescending(movie => movie.ReleaseYear)
        };

        var movies = await moviesQuery.ToListAsync();

        return new AdminMovieIndexViewModel
        {
            Query = query,
            SelectedGenreId = genreId,
            SortBy = sortBy,
            Genres = await GetGenreOptionsAsync(),
            Movies = movies.Select(movie => new AdminMovieListItemViewModel
            {
                Id = movie.Id,
                Title = movie.Title,
                ReleaseYear = movie.ReleaseYear,
                Runtime = movie.Runtime,
                ExternalApiId = movie.ExternalApiId,
                Genres = movie.MovieGenres
                    .Where(movieGenre => movieGenre.Genre is not null)
                    .Select(movieGenre => movieGenre.Genre!.Name)
                    .OrderBy(name => name)
                    .ToArray(),
                WatchlistItemCount = movie.WatchlistItems.Count,
                ViewingHistoryCount = movie.ViewingHistoryItems.Count,
                RatingCount = movie.Ratings.Count,
                ReviewCount = movie.Reviews.Count
            }).ToArray()
        };
    }

    public async Task<AdminMovieDetailsViewModel?> GetMovieDetailsAsync(int movieId)
    {
        var movie = await _context.Movies
            .AsNoTracking()
            .Include(item => item.MovieGenres)
                .ThenInclude(movieGenre => movieGenre.Genre)
            .Include(movie => movie.WatchlistItems)
            .Include(movie => movie.ViewingHistoryItems)
            .Include(movie => movie.Ratings)
            .Include(movie => movie.Reviews)
            .AsSplitQuery()
            .SingleOrDefaultAsync(movie => movie.Id == movieId);

        if (movie is null)
        {
            return null;
        }

        var assignedGenres = movie.MovieGenres
            .Where(movieGenre => movieGenre.Genre is not null)
            .Select(movieGenre => new AdminGenreOptionViewModel
            {
                Id = movieGenre.GenreId,
                Name = movieGenre.Genre!.Name
            })
            .OrderBy(genre => genre.Name)
            .ToArray();

        var assignedGenreIds = assignedGenres.Select(genre => genre.Id).ToHashSet();

        return new AdminMovieDetailsViewModel
        {
            Id = movie.Id,
            Title = movie.Title,
            ReleaseYear = movie.ReleaseYear,
            Description = movie.Description,
            PosterUrl = movie.PosterUrl,
            Runtime = movie.Runtime,
            ExternalApiId = movie.ExternalApiId,
            AssignedGenres = assignedGenres,
            AvailableGenres = (await GetGenreOptionsAsync())
                .Where(genre => !assignedGenreIds.Contains(genre.Id))
                .ToArray(),
            WatchlistItemCount = movie.WatchlistItems.Count,
            ViewingHistoryCount = movie.ViewingHistoryItems.Count,
            RatingCount = movie.Ratings.Count,
            ReviewCount = movie.Reviews.Count
        };
    }

    public async Task<AdminMovieFormViewModel> CreateMovieFormAsync()
    {
        return new AdminMovieFormViewModel
        {
            AvailableGenres = await GetGenreOptionsAsync()
        };
    }

    public async Task<AdminMovieFormViewModel?> GetMovieFormAsync(int movieId)
    {
        var movie = await _context.Movies
            .AsNoTracking()
            .Include(movie => movie.MovieGenres)
            .SingleOrDefaultAsync(movie => movie.Id == movieId);

        if (movie is null)
        {
            return null;
        }

        return new AdminMovieFormViewModel
        {
            Id = movie.Id,
            Title = movie.Title,
            ReleaseYear = movie.ReleaseYear,
            Description = movie.Description,
            PosterUrl = movie.PosterUrl,
            Runtime = movie.Runtime,
            ExternalApiId = movie.ExternalApiId,
            SelectedGenreIds = movie.MovieGenres.Select(movieGenre => movieGenre.GenreId).ToList(),
            AvailableGenres = await GetGenreOptionsAsync()
        };
    }

    public async Task<AdminCatalogResult> CreateMovieAsync(AdminMovieFormViewModel model)
    {
        var normalized = NormalizeMovieForm(model);
        if (await MovieDuplicateExistsAsync(normalized.Title, normalized.ReleaseYear, null))
        {
            return new AdminCatalogResult(false, "A movie with the same title and release year already exists.");
        }

        var externalApiId = NormalizeOptional(model.ExternalApiId);
        if (await ExternalApiIdDuplicateExistsAsync(externalApiId, null))
        {
            return new AdminCatalogResult(false, "A movie with the same external API ID already exists.");
        }

        var movie = new Movie
        {
            Title = normalized.Title,
            ReleaseYear = normalized.ReleaseYear,
            Description = NormalizeOptional(model.Description),
            PosterUrl = NormalizeOptional(model.PosterUrl),
            Runtime = normalized.Runtime,
            ExternalApiId = externalApiId
        };

        _context.Movies.Add(movie);
        await _context.SaveChangesAsync();
        await ReplaceMovieGenresAsync(movie.Id, model.SelectedGenreIds);
        await _context.SaveChangesAsync();

        return new AdminCatalogResult(true, "Movie created.", movie.Id);
    }

    public async Task<AdminCatalogResult> UpdateMovieAsync(AdminMovieFormViewModel model)
    {
        if (!model.Id.HasValue)
        {
            return new AdminCatalogResult(false, "Movie ID is required.");
        }

        var movie = await _context.Movies.FindAsync(model.Id.Value);
        if (movie is null)
        {
            return new AdminCatalogResult(false, "Movie was not found.");
        }

        var normalized = NormalizeMovieForm(model);
        if (await MovieDuplicateExistsAsync(normalized.Title, normalized.ReleaseYear, movie.Id))
        {
            return new AdminCatalogResult(false, "A movie with the same title and release year already exists.");
        }

        var externalApiId = NormalizeOptional(model.ExternalApiId);
        if (await ExternalApiIdDuplicateExistsAsync(externalApiId, movie.Id))
        {
            return new AdminCatalogResult(false, "A movie with the same external API ID already exists.");
        }

        movie.Title = normalized.Title;
        movie.ReleaseYear = normalized.ReleaseYear;
        movie.Description = NormalizeOptional(model.Description);
        movie.PosterUrl = NormalizeOptional(model.PosterUrl);
        movie.Runtime = normalized.Runtime;
        movie.ExternalApiId = externalApiId;

        await ReplaceMovieGenresAsync(movie.Id, model.SelectedGenreIds);
        await _context.SaveChangesAsync();

        return new AdminCatalogResult(true, "Movie updated.", movie.Id);
    }

    public async Task<AdminCatalogResult> DeleteMovieAsync(int movieId)
    {
        var movie = await _context.Movies
            .Include(movie => movie.WatchlistItems)
            .Include(movie => movie.ViewingHistoryItems)
            .Include(movie => movie.Ratings)
            .Include(movie => movie.Reviews)
            .Include(movie => movie.MovieGenres)
            .SingleOrDefaultAsync(movie => movie.Id == movieId);

        if (movie is null)
        {
            return new AdminCatalogResult(false, "Movie was not found.");
        }

        if (movie.WatchlistItems.Count + movie.ViewingHistoryItems.Count + movie.Ratings.Count + movie.Reviews.Count > 0)
        {
            return new AdminCatalogResult(false, "Movie cannot be deleted while watchlist, viewing history, rating, or review records reference it.");
        }

        _context.MovieGenres.RemoveRange(movie.MovieGenres);
        _context.Movies.Remove(movie);
        await _context.SaveChangesAsync();

        return new AdminCatalogResult(true, "Movie deleted.");
    }

    public async Task<AdminCatalogResult> AssignGenreAsync(int movieId, int genreId)
    {
        var movieExists = await _context.Movies.AnyAsync(movie => movie.Id == movieId);
        var genreExists = await _context.Genres.AnyAsync(genre => genre.Id == genreId);

        if (!movieExists || !genreExists)
        {
            return new AdminCatalogResult(false, "Movie or genre was not found.");
        }

        var exists = await _context.MovieGenres.AnyAsync(movieGenre =>
            movieGenre.MovieId == movieId &&
            movieGenre.GenreId == genreId);

        if (exists)
        {
            return new AdminCatalogResult(false, "That genre is already assigned to the movie.");
        }

        _context.MovieGenres.Add(new MovieGenre
        {
            MovieId = movieId,
            GenreId = genreId
        });

        await _context.SaveChangesAsync();
        return new AdminCatalogResult(true, "Genre assigned.", movieId);
    }

    public async Task<AdminCatalogResult> RemoveGenreFromMovieAsync(int movieId, int genreId)
    {
        var movieGenre = await _context.MovieGenres.SingleOrDefaultAsync(item =>
            item.MovieId == movieId &&
            item.GenreId == genreId);

        if (movieGenre is null)
        {
            return new AdminCatalogResult(false, "That genre assignment was not found.");
        }

        _context.MovieGenres.Remove(movieGenre);
        await _context.SaveChangesAsync();
        return new AdminCatalogResult(true, "Genre removed from movie.", movieId);
    }

    public async Task<AdminGenreIndexViewModel> GetGenreIndexAsync()
    {
        var genres = await _context.Genres
            .AsNoTracking()
            .Include(genre => genre.MovieGenres)
            .OrderBy(genre => genre.Name)
            .ToListAsync();

        return new AdminGenreIndexViewModel
        {
            Genres = genres.Select(genre => new AdminGenreListItemViewModel
            {
                Id = genre.Id,
                Name = genre.Name,
                MovieCount = genre.MovieGenres.Count
            }).ToArray()
        };
    }

    public async Task<AdminGenreFormViewModel?> GetGenreFormAsync(int genreId)
    {
        var genre = await _context.Genres
            .AsNoTracking()
            .SingleOrDefaultAsync(genre => genre.Id == genreId);

        return genre is null
            ? null
            : new AdminGenreFormViewModel
            {
                Id = genre.Id,
                Name = genre.Name
            };
    }

    public async Task<AdminCatalogResult> CreateGenreAsync(AdminGenreFormViewModel model)
    {
        var name = NormalizeRequired(model.Name);
        if (await GenreDuplicateExistsAsync(name, null))
        {
            return new AdminCatalogResult(false, "A genre with that name already exists.");
        }

        var genre = new Genre { Name = name };
        _context.Genres.Add(genre);
        await _context.SaveChangesAsync();

        return new AdminCatalogResult(true, "Genre created.", genre.Id);
    }

    public async Task<AdminCatalogResult> UpdateGenreAsync(AdminGenreFormViewModel model)
    {
        if (!model.Id.HasValue)
        {
            return new AdminCatalogResult(false, "Genre ID is required.");
        }

        var genre = await _context.Genres.FindAsync(model.Id.Value);
        if (genre is null)
        {
            return new AdminCatalogResult(false, "Genre was not found.");
        }

        var name = NormalizeRequired(model.Name);
        if (await GenreDuplicateExistsAsync(name, genre.Id))
        {
            return new AdminCatalogResult(false, "A genre with that name already exists.");
        }

        genre.Name = name;
        await _context.SaveChangesAsync();

        return new AdminCatalogResult(true, "Genre updated.", genre.Id);
    }

    public async Task<AdminCatalogResult> DeleteGenreAsync(int genreId)
    {
        var genre = await _context.Genres
            .Include(genre => genre.MovieGenres)
            .SingleOrDefaultAsync(genre => genre.Id == genreId);

        if (genre is null)
        {
            return new AdminCatalogResult(false, "Genre was not found.");
        }

        if (genre.MovieGenres.Count > 0)
        {
            return new AdminCatalogResult(false, "Genre cannot be deleted while it is assigned to movies.");
        }

        _context.Genres.Remove(genre);
        await _context.SaveChangesAsync();

        return new AdminCatalogResult(true, "Genre deleted.");
    }

    private async Task<IReadOnlyList<AdminGenreOptionViewModel>> GetGenreOptionsAsync()
    {
        return await _context.Genres
            .AsNoTracking()
            .OrderBy(genre => genre.Name)
            .Select(genre => new AdminGenreOptionViewModel
            {
                Id = genre.Id,
                Name = genre.Name
            })
            .ToArrayAsync();
    }

    private async Task ReplaceMovieGenresAsync(int movieId, IEnumerable<int> selectedGenreIds)
    {
        var selectedIds = selectedGenreIds
            .Distinct()
            .ToHashSet();

        var validGenreIds = await _context.Genres
            .Where(genre => selectedIds.Contains(genre.Id))
            .Select(genre => genre.Id)
            .ToArrayAsync();

        var existing = await _context.MovieGenres
            .Where(movieGenre => movieGenre.MovieId == movieId)
            .ToListAsync();

        _context.MovieGenres.RemoveRange(existing.Where(movieGenre => !validGenreIds.Contains(movieGenre.GenreId)));

        var existingGenreIds = existing.Select(movieGenre => movieGenre.GenreId).ToHashSet();
        var newLinks = validGenreIds
            .Where(genreId => !existingGenreIds.Contains(genreId))
            .Select(genreId => new MovieGenre
            {
                MovieId = movieId,
                GenreId = genreId
            });

        _context.MovieGenres.AddRange(newLinks);
    }

    private async Task<bool> MovieDuplicateExistsAsync(string title, int? releaseYear, int? currentMovieId)
    {
        var normalizedTitle = title.ToUpperInvariant();

        return await _context.Movies.AnyAsync(movie =>
            (!currentMovieId.HasValue || movie.Id != currentMovieId.Value) &&
            movie.Title.ToUpper() == normalizedTitle &&
            movie.ReleaseYear == releaseYear);
    }

    private async Task<bool> ExternalApiIdDuplicateExistsAsync(string? externalApiId, int? currentMovieId)
    {
        if (string.IsNullOrWhiteSpace(externalApiId))
        {
            return false;
        }

        return await _context.Movies.AnyAsync(movie =>
            (!currentMovieId.HasValue || movie.Id != currentMovieId.Value) &&
            movie.ExternalApiId == externalApiId);
    }

    private async Task<bool> GenreDuplicateExistsAsync(string name, int? currentGenreId)
    {
        var normalizedName = name.ToUpperInvariant();

        return await _context.Genres.AnyAsync(genre =>
            (!currentGenreId.HasValue || genre.Id != currentGenreId.Value) &&
            genre.Name.ToUpper() == normalizedName);
    }

    private static AdminMovieFormViewModel NormalizeMovieForm(AdminMovieFormViewModel model)
    {
        model.Title = NormalizeRequired(model.Title);
        return model;
    }

    private static string NormalizeRequired(string value)
    {
        return value.Trim();
    }

    private static string? NormalizeOptional(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }
}
