using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using MovieWatchlistTracker.Web.Models;

namespace MovieWatchlistTracker.Web.ViewModels;

public class AdminAppearanceViewModel
{
    private const string HexColorPattern = "^#(?:[0-9a-fA-F]{3}|[0-9a-fA-F]{6})$";

    [ValidateNever]
    public string? CurrentLogoPath { get; set; }

    [ValidateNever]
    public string? CurrentBannerPath { get; set; }

    [Display(Name = "Edit Logo")]
    public IFormFile? LogoUpload { get; set; }

    [Display(Name = "Edit Banner")]
    public IFormFile? BannerUpload { get; set; }

    [Display(Name = "Remove Logo")]
    public bool RemoveLogo { get; set; }

    [Display(Name = "Remove Banner")]
    public bool RemoveBanner { get; set; }

    [Required]
    [RegularExpression(HexColorPattern, ErrorMessage = "Use a hex colour like #39ff14.")]
    [Display(Name = "Edit Page Colour")]
    public string PageBackgroundColor { get; set; } = SiteAppearanceSettingsDefaults.PageBackgroundColor;

    [Required]
    [RegularExpression(HexColorPattern, ErrorMessage = "Use a hex colour like #39ff14.")]
    [Display(Name = "Edit Text Colour")]
    public string TextColor { get; set; } = SiteAppearanceSettingsDefaults.TextColor;

    [Required]
    [RegularExpression(HexColorPattern, ErrorMessage = "Use a hex colour like #39ff14.")]
    [Display(Name = "Edit Navigation Bar Text Colour")]
    public string NavigationBarTextColor { get; set; } = SiteAppearanceSettingsDefaults.NavigationBarTextColor;

    [Required]
    [RegularExpression(HexColorPattern, ErrorMessage = "Use a hex colour like #ff1744.")]
    [Display(Name = "Edit Browse Page Heading Colour")]
    public string BrowsePageHeadingColor { get; set; } = SiteAppearanceSettingsDefaults.BrowsePageHeadingColor;

    [Required]
    [RegularExpression(HexColorPattern, ErrorMessage = "Use a hex colour like #39ff14.")]
    [Display(Name = "Edit Outline Colour")]
    public string OutlineColor { get; set; } = SiteAppearanceSettingsDefaults.OutlineColor;

    [Required]
    [RegularExpression(HexColorPattern, ErrorMessage = "Use a hex colour like #ff1744.")]
    [Display(Name = "Edit Option Button Outline Colour")]
    public string OptionButtonOutlineColor { get; set; } = SiteAppearanceSettingsDefaults.OptionButtonOutlineColor;

    [Required]
    [RegularExpression(HexColorPattern, ErrorMessage = "Use a hex colour like #00fff7.")]
    [Display(Name = "Edit Option Button Text Colour")]
    public string OptionButtonTextColor { get; set; } = SiteAppearanceSettingsDefaults.OptionButtonTextColor;
}
