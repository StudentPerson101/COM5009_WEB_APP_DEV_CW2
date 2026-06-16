namespace MovieWatchlistTracker.Web.ViewModels;

public class MovieDetailsViewModel
{
    public int MovieId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int? ReleaseYear { get; set; }
    public string? Description { get; set; }
    public string? PosterUrl { get; set; }
    public int? Runtime { get; set; }
    public string DisplayDuration => MovieDurationFormatter.Format(Runtime, "Duration TBA");
    public IReadOnlyList<string> Genres { get; set; } = [];
    public double? AverageRating { get; set; }
    public int? CurrentUserRatingId { get; set; }
    public double? CurrentUserRating { get; set; }
    public int? CurrentUserReviewId { get; set; }
    public string? CurrentUserReview { get; set; }
    public bool IsInWatchlist { get; set; }
    public bool IsWatched { get; set; }
    public IReadOnlyList<ReviewDisplayViewModel> OtherReviews { get; set; } = [];
}
