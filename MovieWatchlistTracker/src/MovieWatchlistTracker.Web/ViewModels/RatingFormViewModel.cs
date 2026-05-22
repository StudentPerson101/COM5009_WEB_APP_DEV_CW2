using System.ComponentModel.DataAnnotations;

namespace MovieWatchlistTracker.Web.ViewModels;

public class RatingFormViewModel
{
    [Required]
    public int MovieId { get; set; }

    [Range(1, 5)]
    public int Score { get; set; }
}
