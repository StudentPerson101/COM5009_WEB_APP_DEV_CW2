namespace MovieWatchlistTracker.Web.ViewModels;

public class AdminGenreIndexViewModel
{
    public IReadOnlyList<AdminGenreListItemViewModel> Genres { get; set; } = [];
}

public class AdminGenreListItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int MovieCount { get; set; }
}
