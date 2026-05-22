namespace MovieWatchlistTracker.Web.Models;

public class Watchlist
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    public string Name { get; set; } = "My Watchlist";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<WatchlistItem> Items { get; set; } = [];
}
