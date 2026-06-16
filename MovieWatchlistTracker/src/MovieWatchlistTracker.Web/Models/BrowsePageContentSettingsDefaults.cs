namespace MovieWatchlistTracker.Web.Models;

public static class BrowsePageContentSettingsDefaults
{
    public const string EyebrowText = "REALLY GREAT BLOCKBUSTERS";
    public const string HeadingText = "Discover your next favourite movie";

    public static BrowsePageContentSettings Create() => new()
    {
        Id = BrowsePageContentSettings.SingletonId,
        EyebrowText = EyebrowText,
        HeadingText = HeadingText
    };
}
