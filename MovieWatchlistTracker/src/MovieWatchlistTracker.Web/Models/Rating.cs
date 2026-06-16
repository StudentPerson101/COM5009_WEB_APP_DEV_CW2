namespace MovieWatchlistTracker.Web.Models;

public class Rating
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    public int MovieId { get; set; }
    public Movie? Movie { get; set; }

    public double Score { get; set; }
}
