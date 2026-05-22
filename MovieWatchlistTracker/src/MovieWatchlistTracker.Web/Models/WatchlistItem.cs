namespace MovieWatchlistTracker.Web.Models;

public class WatchlistItem
{
    public int Id { get; set; }

    public int WatchlistId { get; set; }
    public Watchlist? Watchlist { get; set; }

    public int MovieId { get; set; }
    public Movie? Movie { get; set; }

    public WatchlistItemStatus Status { get; set; } = WatchlistItemStatus.Planned;
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}
