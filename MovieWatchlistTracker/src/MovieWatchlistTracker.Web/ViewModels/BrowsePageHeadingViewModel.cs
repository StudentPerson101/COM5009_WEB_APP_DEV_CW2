using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using MovieWatchlistTracker.Web.Models;

namespace MovieWatchlistTracker.Web.ViewModels;

public class BrowsePageHeadingViewModel
{
    [Required]
    [StringLength(80)]
    [Display(Name = "Edit Browse Eyebrow Text")]
    public string EyebrowText { get; set; } = BrowsePageContentSettingsDefaults.EyebrowText;

    [Required]
    [StringLength(140)]
    [Display(Name = "Edit Browse Heading Text")]
    public string HeadingText { get; set; } = BrowsePageContentSettingsDefaults.HeadingText;

    [ValidateNever]
    public string? ReturnUrl { get; set; }
}
