namespace MovieWatchlistTracker.Web.ViewModels;

public class WatchlistItemViewModel
{
    public int? WatchlistItemId { get; set; }
    public int MovieId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int? ReleaseYear { get; set; }
    public string? PosterUrl { get; set; }
    public IReadOnlyList<string> Genres { get; set; } = [];
    public double? AverageRating { get; set; }
    public string Status { get; set; } = "planned";
    public DateTime AddedAt { get; set; }
    public DateTime? WatchedAt { get; set; }
}
