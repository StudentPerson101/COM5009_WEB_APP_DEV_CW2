using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MovieWatchlistTracker.Web.Models;

namespace MovieWatchlistTracker.Web.Data;

public static class DbInitializer
{
    public static void Initialize(ApplicationDbContext context)
    {
        context.Database.Migrate();

        var sampleUser = EnsureSampleUser(context);
        var adminRole = EnsureAdminRole(context);
        EnsureSampleUserAdminRole(context, sampleUser.Id, adminRole.Id);
        PruneLegacyDemoCatalog(context);
        var genresByName = EnsureGenres(context);
        var moviesByCatalogKey = EnsureMovies(context);

        EnsureMovieGenres(context, moviesByCatalogKey, genresByName);
        PruneUnusedLegacyGenres(context);

        var defaultWatchlist = EnsureDefaultWatchlist(context, sampleUser.Id);
        EnsureWatchlistItems(context, defaultWatchlist.Id, moviesByCatalogKey);
        EnsureViewingHistory(context, sampleUser.Id, moviesByCatalogKey);
        EnsureReviews(context, sampleUser.Id, moviesByCatalogKey);

        context.SaveChanges();
    }

    private static void PruneLegacyDemoCatalog(ApplicationDbContext context)
    {
        foreach (var (title, releaseYear) in SeedData.LegacyDemoMovies)
        {
            var legacyMovies = context.Movies
                .Where(movie => movie.Title == title && movie.ReleaseYear == releaseYear)
                .ToList();

            context.Movies.RemoveRange(legacyMovies);
        }

        context.SaveChanges();
    }

    private static ApplicationUser EnsureSampleUser(ApplicationDbContext context)
    {
        var sampleUser = context.Users.SingleOrDefault(user => user.Id == SeedData.SampleUserId);

        if (sampleUser is not null)
        {
            return sampleUser;
        }

        sampleUser = new ApplicationUser
        {
            Id = SeedData.SampleUserId,
            UserName = SeedData.SampleUserName,
            NormalizedUserName = SeedData.SampleUserName.ToUpperInvariant(),
            Email = SeedData.SampleUserEmail,
            NormalizedEmail = SeedData.SampleUserEmail.ToUpperInvariant(),
            EmailConfirmed = true,
            CreatedAt = SeedData.SeededAt,
            SecurityStamp = Guid.NewGuid().ToString(),
            ConcurrencyStamp = Guid.NewGuid().ToString()
        };

        sampleUser.PasswordHash = new PasswordHasher<ApplicationUser>()
            .HashPassword(sampleUser, SeedData.SampleUserPassword);

        context.Users.Add(sampleUser);
        context.SaveChanges();

        return sampleUser;
    }

    private static IdentityRole EnsureAdminRole(ApplicationDbContext context)
    {
        var adminRole = context.Roles.SingleOrDefault(role => role.Id == SeedData.AdminRoleId);

        if (adminRole is not null)
        {
            adminRole.Name = SeedData.AdminRoleName;
            adminRole.NormalizedName = SeedData.AdminRoleName.ToUpperInvariant();
            return adminRole;
        }

        adminRole = new IdentityRole
        {
            Id = SeedData.AdminRoleId,
            Name = SeedData.AdminRoleName,
            NormalizedName = SeedData.AdminRoleName.ToUpperInvariant(),
            ConcurrencyStamp = Guid.NewGuid().ToString()
        };

        context.Roles.Add(adminRole);
        context.SaveChanges();

        return adminRole;
    }

    private static void EnsureSampleUserAdminRole(ApplicationDbContext context, string userId, string roleId)
    {
        var exists = context.UserRoles.Any(userRole =>
            userRole.UserId == userId &&
            userRole.RoleId == roleId);

        if (exists)
        {
            return;
        }

        context.UserRoles.Add(new IdentityUserRole<string>
        {
            UserId = userId,
            RoleId = roleId
        });

        context.SaveChanges();
    }

    private static Dictionary<string, Genre> EnsureGenres(ApplicationDbContext context)
    {
        foreach (var seedGenre in SeedData.Genres)
        {
            if (!context.Genres.Any(genre => genre.Name == seedGenre.Name))
            {
                context.Genres.Add(seedGenre);
            }
        }

        context.SaveChanges();

        return context.Genres
            .Where(genre => SeedData.Genres.Select(seedGenre => seedGenre.Name).Contains(genre.Name))
            .ToDictionary(genre => genre.Name);
    }

    private static Dictionary<string, Movie> EnsureMovies(ApplicationDbContext context)
    {
        foreach (var seedDefinition in SeedData.Movies)
        {
            var seedMovie = seedDefinition.Movie;
            var existingMovie = context.Movies.FirstOrDefault(movie =>
                movie.Title == seedMovie.Title &&
                movie.ReleaseYear == seedMovie.ReleaseYear);

            if (existingMovie is null)
            {
                context.Movies.Add(seedMovie);
                continue;
            }

            existingMovie.Description = seedMovie.Description;
            if (string.IsNullOrWhiteSpace(existingMovie.PosterUrl))
            {
                existingMovie.PosterUrl = seedMovie.PosterUrl;
            }
            existingMovie.Runtime = seedMovie.Runtime;
            existingMovie.ExternalApiId = null;
        }

        context.SaveChanges();

        return SeedData.Movies
            .Select(seedDefinition => new
            {
                seedDefinition.CatalogKey,
                Movie = context.Movies.First(movie =>
                    movie.Title == seedDefinition.Movie.Title &&
                    movie.ReleaseYear == seedDefinition.Movie.ReleaseYear)
            })
            .ToDictionary(seedDefinition => seedDefinition.CatalogKey, seedDefinition => seedDefinition.Movie);
    }

    private static void EnsureMovieGenres(
        ApplicationDbContext context,
        IReadOnlyDictionary<string, Movie> moviesByCatalogKey,
        IReadOnlyDictionary<string, Genre> genresByName)
    {
        foreach (var (catalogKey, genreNames) in SeedData.MovieGenres)
        {
            if (!moviesByCatalogKey.TryGetValue(catalogKey, out var movie))
            {
                continue;
            }

            var desiredGenreIds = genreNames
                .Where(genresByName.ContainsKey)
                .Select(genreName => genresByName[genreName].Id)
                .ToHashSet();

            var staleMovieGenres = context.MovieGenres
                .Where(movieGenre =>
                    movieGenre.MovieId == movie.Id &&
                    !desiredGenreIds.Contains(movieGenre.GenreId))
                .ToList();

            context.MovieGenres.RemoveRange(staleMovieGenres);

            foreach (var genreName in genreNames)
            {
                if (!genresByName.TryGetValue(genreName, out var genre))
                {
                    continue;
                }

                var exists = context.MovieGenres.Any(movieGenre =>
                    movieGenre.MovieId == movie.Id &&
                    movieGenre.GenreId == genre.Id);

                if (!exists)
                {
                    context.MovieGenres.Add(new MovieGenre
                    {
                        MovieId = movie.Id,
                        GenreId = genre.Id
                    });
                }
            }
        }

        context.SaveChanges();
    }

    private static void PruneUnusedLegacyGenres(ApplicationDbContext context)
    {
        var currentSeedGenreNames = SeedData.Genres
            .Select(genre => genre.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var unusedLegacyGenres = context.Genres
            .Where(genre =>
                SeedData.LegacyDemoGenres.Contains(genre.Name) &&
                !currentSeedGenreNames.Contains(genre.Name) &&
                !genre.MovieGenres.Any())
            .ToList();

        context.Genres.RemoveRange(unusedLegacyGenres);
        context.SaveChanges();
    }

    private static Watchlist EnsureDefaultWatchlist(ApplicationDbContext context, string userId)
    {
        var watchlist = context.Watchlists.SingleOrDefault(existing =>
            existing.UserId == userId &&
            existing.Name == SeedData.DefaultWatchlistName);

        if (watchlist is not null)
        {
            return watchlist;
        }

        watchlist = new Watchlist
        {
            UserId = userId,
            Name = SeedData.DefaultWatchlistName,
            CreatedAt = SeedData.SeededAt
        };

        context.Watchlists.Add(watchlist);
        context.SaveChanges();

        return watchlist;
    }

    private static void EnsureWatchlistItems(
        ApplicationDbContext context,
        int watchlistId,
        IReadOnlyDictionary<string, Movie> moviesByCatalogKey)
    {
        var addedAt = SeedData.SeededAt;

        foreach (var (catalogKey, status) in SeedData.WatchlistStatuses)
        {
            if (!moviesByCatalogKey.TryGetValue(catalogKey, out var movie))
            {
                continue;
            }

            var exists = context.WatchlistItems.Any(item =>
                item.WatchlistId == watchlistId &&
                item.MovieId == movie.Id);

            if (!exists)
            {
                context.WatchlistItems.Add(new WatchlistItem
                {
                    WatchlistId = watchlistId,
                    MovieId = movie.Id,
                    Status = status,
                    AddedAt = addedAt
                });
            }
            else
            {
                var item = context.WatchlistItems.Single(existing =>
                    existing.WatchlistId == watchlistId &&
                    existing.MovieId == movie.Id);

                item.Status = status;
            }

            addedAt = addedAt.AddDays(1);
        }
    }

    private static void EnsureViewingHistory(
        ApplicationDbContext context,
        string userId,
        IReadOnlyDictionary<string, Movie> moviesByCatalogKey)
    {
        var watchedMovieIds = SeedData.WatchlistStatuses
            .Where(pair => pair.Value == WatchlistItemStatus.Watched)
            .Select(pair => pair.Key)
            .Where(moviesByCatalogKey.ContainsKey)
            .Select(catalogKey => moviesByCatalogKey[catalogKey].Id)
            .ToArray();

        var watchedAt = SeedData.SeededAt.AddDays(14);

        foreach (var movieId in watchedMovieIds)
        {
            var historyItem = context.ViewingHistoryItems.SingleOrDefault(item =>
                item.UserId == userId &&
                item.MovieId == movieId);

            if (historyItem is null)
            {
                context.ViewingHistoryItems.Add(new ViewingHistoryItem
                {
                    UserId = userId,
                    MovieId = movieId,
                    WatchedAt = watchedAt
                });
            }
            else
            {
                historyItem.WatchedAt = watchedAt;
            }

            watchedAt = watchedAt.AddDays(3);
        }
    }

    private static void EnsureReviews(
        ApplicationDbContext context,
        string userId,
        IReadOnlyDictionary<string, Movie> moviesByCatalogKey)
    {
        var createdAt = SeedData.SeededAt.AddDays(21);

        foreach (var (catalogKey, text) in SeedData.Reviews)
        {
            if (!moviesByCatalogKey.TryGetValue(catalogKey, out var movie))
            {
                continue;
            }

            var review = context.Reviews.SingleOrDefault(existing =>
                existing.UserId == userId &&
                existing.MovieId == movie.Id);

            if (review is null)
            {
                context.Reviews.Add(new Review
                {
                    UserId = userId,
                    MovieId = movie.Id,
                    Text = text,
                    CreatedAt = createdAt,
                    UpdatedAt = createdAt
                });
            }
            else
            {
                review.Text = text;
                review.UpdatedAt = createdAt;
            }

            createdAt = createdAt.AddDays(2);
        }
    }
}
