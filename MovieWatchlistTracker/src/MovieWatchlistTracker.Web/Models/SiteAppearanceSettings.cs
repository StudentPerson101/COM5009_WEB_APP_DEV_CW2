namespace MovieWatchlistTracker.Web.Models;

public class SiteAppearanceSettings
{
    public const int SingletonId = 1;

    public int Id { get; set; } = SingletonId;

    public string? LogoPath { get; set; }

    public string? BannerPath { get; set; }

    public string PageBackgroundColor { get; set; } = SiteAppearanceSettingsDefaults.PageBackgroundColor;

    public string TextColor { get; set; } = SiteAppearanceSettingsDefaults.TextColor;

    public string NavigationBarTextColor { get; set; } = SiteAppearanceSettingsDefaults.NavigationBarTextColor;

    public string BrowsePageHeadingColor { get; set; } = SiteAppearanceSettingsDefaults.BrowsePageHeadingColor;

    public string OutlineColor { get; set; } = SiteAppearanceSettingsDefaults.OutlineColor;

    public string OptionButtonOutlineColor { get; set; } = SiteAppearanceSettingsDefaults.OptionButtonOutlineColor;

    public string OptionButtonTextColor { get; set; } = SiteAppearanceSettingsDefaults.OptionButtonTextColor;
}
