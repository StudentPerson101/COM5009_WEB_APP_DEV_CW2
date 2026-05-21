namespace MovieWatchlistTracker.Web.Models;

public class Movie
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int? ReleaseYear { get; set; }
    public string? Description { get; set; }
    public string? PosterUrl { get; set; }
    public int? Runtime { get; set; }
    public string? ExternalApiId { get; set; }

    public ICollection<MovieGenre> MovieGenres { get; set; } = [];
    public ICollection<WatchlistItem> WatchlistItems { get; set; } = [];
    public ICollection<ViewingHistoryItem> ViewingHistoryItems { get; set; } = [];
    public ICollection<Rating> Ratings { get; set; } = [];
    public ICollection<Review> Reviews { get; set; } = [];
}
