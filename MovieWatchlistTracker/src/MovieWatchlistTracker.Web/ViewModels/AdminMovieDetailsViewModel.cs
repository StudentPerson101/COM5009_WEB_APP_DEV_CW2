namespace MovieWatchlistTracker.Web.ViewModels;

public class AdminMovieDetailsViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int? ReleaseYear { get; set; }
    public string? Description { get; set; }
    public string? PosterUrl { get; set; }
    public int? Runtime { get; set; }
    public string DisplayDuration => MovieDurationFormatter.Format(Runtime, "Not set");
    public string? ExternalApiId { get; set; }
    public IReadOnlyList<AdminGenreOptionViewModel> AssignedGenres { get; set; } = [];
    public IReadOnlyList<AdminGenreOptionViewModel> AvailableGenres { get; set; } = [];
    public int WatchlistItemCount { get; set; }
    public int ViewingHistoryCount { get; set; }
    public int RatingCount { get; set; }
    public int ReviewCount { get; set; }
    public bool HasUserData => WatchlistItemCount + ViewingHistoryCount + RatingCount + ReviewCount > 0;
}
