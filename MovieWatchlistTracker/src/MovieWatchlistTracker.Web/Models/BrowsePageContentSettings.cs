namespace MovieWatchlistTracker.Web.Models;

public class BrowsePageContentSettings
{
    public const int SingletonId = 1;

    public int Id { get; set; } = SingletonId;

    public string EyebrowText { get; set; } = BrowsePageContentSettingsDefaults.EyebrowText;

    public string HeadingText { get; set; } = BrowsePageContentSettingsDefaults.HeadingText;
}
