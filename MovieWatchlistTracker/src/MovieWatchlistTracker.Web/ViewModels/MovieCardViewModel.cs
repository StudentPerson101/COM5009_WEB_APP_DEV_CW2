namespace MovieWatchlistTracker.Web.ViewModels;

public class MovieCardViewModel
{
    public int MovieId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int? ReleaseYear { get; set; }
    public string? PosterUrl { get; set; }
    public IReadOnlyList<string> Genres { get; set; } = [];
    public int? Runtime { get; set; }
    public string DisplayDuration => MovieDurationFormatter.Format(Runtime, "Duration TBA");
    public decimal? AverageRating { get; set; }
    public bool IsInCurrentUserWatchlist { get; set; }
}
