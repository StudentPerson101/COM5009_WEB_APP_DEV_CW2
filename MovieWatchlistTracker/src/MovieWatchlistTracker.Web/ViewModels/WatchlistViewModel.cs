namespace MovieWatchlistTracker.Web.ViewModels;

public class WatchlistViewModel
{
    public IReadOnlyList<WatchlistItemViewModel> Items { get; set; } = [];
    public string? Query { get; set; }
    public string? SortBy { get; set; }
    public int? SelectedGenreId { get; set; }
    public int? SelectedYear { get; set; }
    public int? SelectedRating { get; set; }
    public string? SelectedStatus { get; set; }
    public IReadOnlyList<GenreFilterOptionViewModel> Genres { get; set; } = [];
}
