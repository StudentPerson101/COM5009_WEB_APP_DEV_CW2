namespace MovieWatchlistTracker.Web.ViewModels;

public static class RatingDisplayFormatter
{
    public static string Format(double rating)
    {
        return rating.ToString("0.0");
    }

    public static string Format(double? rating)
    {
        return rating.HasValue
            ? Format(rating.Value)
            : string.Empty;
    }
}
