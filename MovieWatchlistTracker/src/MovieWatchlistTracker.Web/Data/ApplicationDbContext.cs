using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MovieWatchlistTracker.Web.Models;

namespace MovieWatchlistTracker.Web.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Movie> Movies => Set<Movie>();
    public DbSet<Genre> Genres => Set<Genre>();
    public DbSet<MovieGenre> MovieGenres => Set<MovieGenre>();
    public DbSet<Watchlist> Watchlists => Set<Watchlist>();
    public DbSet<WatchlistItem> WatchlistItems => Set<WatchlistItem>();
    public DbSet<ViewingHistoryItem> ViewingHistoryItems => Set<ViewingHistoryItem>();
    public DbSet<Rating> Ratings => Set<Rating>();
    public DbSet<Review> Reviews => Set<Review>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        ConfigureApplicationUser(builder);
        ConfigureMovie(builder);
        ConfigureGenre(builder);
        ConfigureMovieGenre(builder);
        ConfigureWatchlist(builder);
        ConfigureWatchlistItem(builder);
        ConfigureViewingHistoryItem(builder);
        ConfigureRating(builder);
        ConfigureReview(builder);
    }

    private static void ConfigureApplicationUser(ModelBuilder builder)
    {
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(user => user.CreatedAt)
                .IsRequired();

            entity.HasIndex(user => user.NormalizedEmail)
                .HasDatabaseName("EmailIndex")
                .IsUnique();

            entity.HasMany(user => user.Watchlists)
                .WithOne(watchlist => watchlist.User)
                .HasForeignKey(watchlist => watchlist.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(user => user.ViewingHistoryItems)
                .WithOne(item => item.User)
                .HasForeignKey(item => item.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(user => user.Ratings)
                .WithOne(rating => rating.User)
                .HasForeignKey(rating => rating.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(user => user.Reviews)
                .WithOne(review => review.User)
                .HasForeignKey(review => review.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureMovie(ModelBuilder builder)
    {
        builder.Entity<Movie>(entity =>
        {
            entity.ToTable("Movies", table =>
            {
                table.HasCheckConstraint(
                    "CK_Movies_ReleaseYear",
                    "ReleaseYear IS NULL OR ReleaseYear >= 1888");

                table.HasCheckConstraint(
                    "CK_Movies_Runtime",
                    "Runtime IS NULL OR Runtime > 0");
            });

            entity.Property(movie => movie.Title)
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(movie => movie.ExternalApiId)
                .HasMaxLength(100);

            entity.HasIndex(movie => movie.Title);

            entity.HasIndex(movie => movie.ExternalApiId)
                .IsUnique()
                .HasFilter(null);
        });
    }

    private static void ConfigureGenre(ModelBuilder builder)
    {
        builder.Entity<Genre>(entity =>
        {
            entity.ToTable("Genres");

            entity.Property(genre => genre.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.HasIndex(genre => genre.Name)
                .IsUnique();
        });
    }

    private static void ConfigureMovieGenre(ModelBuilder builder)
    {
        builder.Entity<MovieGenre>(entity =>
        {
            entity.ToTable("MovieGenres");

            entity.HasKey(movieGenre => new
            {
                movieGenre.MovieId,
                movieGenre.GenreId
            });

            entity.HasOne(movieGenre => movieGenre.Movie)
                .WithMany(movie => movie.MovieGenres)
                .HasForeignKey(movieGenre => movieGenre.MovieId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(movieGenre => movieGenre.Genre)
                .WithMany(genre => genre.MovieGenres)
                .HasForeignKey(movieGenre => movieGenre.GenreId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(movieGenre => movieGenre.GenreId);
        });
    }

    private static void ConfigureWatchlist(ModelBuilder builder)
    {
        builder.Entity<Watchlist>(entity =>
        {
            entity.ToTable("Watchlists");

            entity.Property(watchlist => watchlist.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(watchlist => watchlist.CreatedAt)
                .IsRequired();

            entity.HasIndex(watchlist => watchlist.UserId);

            entity.HasIndex(watchlist => new
                {
                    watchlist.UserId,
                    watchlist.Name
                })
                .IsUnique();
        });
    }

    private static void ConfigureWatchlistItem(ModelBuilder builder)
    {
        builder.Entity<WatchlistItem>(entity =>
        {
            entity.ToTable("WatchlistItems", table =>
            {
                table.HasCheckConstraint(
                    "CK_WatchlistItems_Status",
                    "Status IN ('planned', 'watching', 'watched', 'dropped')");
            });

            entity.Property(item => item.Status)
                .HasConversion(
                    status => status.ToString().ToLowerInvariant(),
                    value => Enum.Parse<WatchlistItemStatus>(value, ignoreCase: true))
                .HasMaxLength(30)
                .HasDefaultValue(WatchlistItemStatus.Planned)
                .IsRequired();

            entity.Property(item => item.AddedAt)
                .IsRequired();

            entity.HasOne(item => item.Watchlist)
                .WithMany(watchlist => watchlist.Items)
                .HasForeignKey(item => item.WatchlistId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(item => item.Movie)
                .WithMany(movie => movie.WatchlistItems)
                .HasForeignKey(item => item.MovieId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(item => item.MovieId);

            entity.HasIndex(item => new
                {
                    item.WatchlistId,
                    item.MovieId
                })
                .IsUnique();
        });
    }

    private static void ConfigureViewingHistoryItem(ModelBuilder builder)
    {
        builder.Entity<ViewingHistoryItem>(entity =>
        {
            entity.ToTable("ViewingHistoryItems");

            entity.Property(item => item.WatchedAt)
                .IsRequired();

            entity.HasOne(item => item.Movie)
                .WithMany(movie => movie.ViewingHistoryItems)
                .HasForeignKey(item => item.MovieId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(item => item.UserId);
            entity.HasIndex(item => item.MovieId);
        });
    }

    private static void ConfigureRating(ModelBuilder builder)
    {
        builder.Entity<Rating>(entity =>
        {
            entity.ToTable("Ratings", table =>
            {
                table.HasCheckConstraint(
                    "CK_Ratings_Score",
                    "Score BETWEEN 1 AND 5");
            });

            entity.HasOne(rating => rating.Movie)
                .WithMany(movie => movie.Ratings)
                .HasForeignKey(rating => rating.MovieId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(rating => rating.MovieId);

            entity.HasIndex(rating => new
                {
                    rating.UserId,
                    rating.MovieId
                })
                .IsUnique();
        });
    }

    private static void ConfigureReview(ModelBuilder builder)
    {
        builder.Entity<Review>(entity =>
        {
            entity.ToTable("Reviews");

            entity.Property(review => review.Text)
                .HasMaxLength(4000)
                .IsRequired();

            entity.Property(review => review.CreatedAt)
                .IsRequired();

            entity.Property(review => review.UpdatedAt)
                .IsRequired();

            entity.HasOne(review => review.Movie)
                .WithMany(movie => movie.Reviews)
                .HasForeignKey(review => review.MovieId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(review => review.MovieId);

            entity.HasIndex(review => new
                {
                    review.UserId,
                    review.MovieId
                })
                .IsUnique();
        });
    }
}
