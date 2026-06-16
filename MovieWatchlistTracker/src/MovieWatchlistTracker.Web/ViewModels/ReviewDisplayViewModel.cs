namespace MovieWatchlistTracker.Web.ViewModels;

public class ReviewDisplayViewModel
{
    public int Id { get; set; }
    public string ReviewerName { get; set; } = string.Empty;
    public double? Rating { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsCurrentUserReview { get; set; }
}
