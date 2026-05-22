using Microsoft.AspNetCore.Identity;

namespace MovieWatchlistTracker.Web.Models;

public class ApplicationUser : IdentityUser
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Watchlist> Watchlists { get; set; } = [];
    public ICollection<ViewingHistoryItem> ViewingHistoryItems { get; set; } = [];
    public ICollection<Rating> Ratings { get; set; } = [];
    public ICollection<Review> Reviews { get; set; } = [];
}
