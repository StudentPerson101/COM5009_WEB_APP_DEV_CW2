namespace MovieWatchlistTracker.Web.Models;

public static class SiteAppearanceSettingsDefaults
{
    public const string PageBackgroundColor = "#000000";
    public const string TextColor = "#39ff14";
    public const string NavigationBarTextColor = "#39ff14";
    public const string BrowsePageHeadingColor = "#ff1744";
    public const string OutlineColor = "#39ff14";
    public const string OptionButtonOutlineColor = "#ff1744";
    public const string OptionButtonTextColor = "#00fff7";
    public const string LogoPath = "/images/branding/default-logo.jpg";
    public const string BannerPath = "/images/branding/default-banner.jpg";

    public static SiteAppearanceSettings Create() => new()
    {
        Id = SiteAppearanceSettings.SingletonId,
        LogoPath = LogoPath,
        BannerPath = BannerPath,
        PageBackgroundColor = PageBackgroundColor,
        TextColor = TextColor,
        NavigationBarTextColor = NavigationBarTextColor,
        BrowsePageHeadingColor = BrowsePageHeadingColor,
        OutlineColor = OutlineColor,
        OptionButtonOutlineColor = OptionButtonOutlineColor,
        OptionButtonTextColor = OptionButtonTextColor
    };
}
