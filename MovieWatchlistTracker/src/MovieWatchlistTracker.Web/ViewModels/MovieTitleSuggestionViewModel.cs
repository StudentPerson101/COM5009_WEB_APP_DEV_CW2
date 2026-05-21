namespace MovieWatchlistTracker.Web.ViewModels;

public class MovieTitleSuggestionViewModel
{
    public int MovieId { get; set; }

    public string Title { get; set; } = string.Empty;

    public int? ReleaseYear { get; set; }
}
