namespace MovieWatchlistTracker.Web.ViewModels;

public class AdminMovieIndexViewModel
{
    public string? Query { get; set; }
    public int? SelectedGenreId { get; set; }
    public string? SortBy { get; set; }
    public IReadOnlyList<AdminGenreOptionViewModel> Genres { get; set; } = [];
    public IReadOnlyList<AdminMovieListItemViewModel> Movies { get; set; } = [];
}

public class AdminMovieListItemViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int? ReleaseYear { get; set; }
    public int? Runtime { get; set; }
    public string DisplayDuration => MovieDurationFormatter.Format(Runtime, "Unknown");
    public string? ExternalApiId { get; set; }
    public IReadOnlyList<string> Genres { get; set; } = [];
    public int WatchlistItemCount { get; set; }
    public int ViewingHistoryCount { get; set; }
    public int RatingCount { get; set; }
    public int ReviewCount { get; set; }
}
