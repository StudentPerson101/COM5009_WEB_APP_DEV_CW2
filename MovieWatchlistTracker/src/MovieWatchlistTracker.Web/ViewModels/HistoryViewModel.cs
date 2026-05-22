namespace MovieWatchlistTracker.Web.ViewModels;

public class HistoryViewModel
{
    public IReadOnlyList<WatchlistItemViewModel> Items { get; set; } = [];
    public string? SortBy { get; set; }
    public int? SelectedGenreId { get; set; }
    public IReadOnlyList<GenreFilterOptionViewModel> Genres { get; set; } = [];
}
