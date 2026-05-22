namespace MovieWatchlistTracker.Web.ViewModels;

public class ProfileViewModel
{
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int WatchlistCount { get; set; }
    public int WatchedCount { get; set; }
    public int RatingCount { get; set; }
    public int ReviewCount { get; set; }
}
