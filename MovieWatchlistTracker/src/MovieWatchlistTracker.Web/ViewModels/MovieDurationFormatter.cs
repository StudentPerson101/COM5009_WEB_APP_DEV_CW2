namespace MovieWatchlistTracker.Web.ViewModels;

internal static class MovieDurationFormatter
{
    public static string Format(int? durationMinutes, string fallback)
    {
        return durationMinutes.HasValue
            ? Format(durationMinutes.Value)
            : fallback;
    }

    private static string Format(int durationMinutes)
    {
        var duration = TimeSpan.FromMinutes(durationMinutes);
        var totalHours = (int)Math.Floor(duration.TotalHours);

        return $"{totalHours:00}:{duration.Minutes:00}:{duration.Seconds:00}";
    }
}
