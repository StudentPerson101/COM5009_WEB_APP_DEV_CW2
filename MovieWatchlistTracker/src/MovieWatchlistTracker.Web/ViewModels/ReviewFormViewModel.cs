using System.ComponentModel.DataAnnotations;

namespace MovieWatchlistTracker.Web.ViewModels;

public class ReviewFormViewModel
{
    [Required]
    public int MovieId { get; set; }

    [Required]
    [StringLength(4000)]
    [Display(Name = "Review")]
    public string Text { get; set; } = string.Empty;
}
