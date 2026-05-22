namespace MovieWatchlistTracker.Web.ViewModels;

public class MovieSearchViewModel
{
    public string? Query { get; set; }
    public int? SelectedGenreId { get; set; }
    public int? SelectedYear { get; set; }
    public int? SelectedRating { get; set; }
    public string? SelectedWatchedStatus { get; set; }
    public string? SortBy { get; set; }
    public IReadOnlyList<MovieCardViewModel> Movies { get; set; } = [];
    public IReadOnlyList<GenreFilterOptionViewModel> Genres { get; set; } = [];
}
