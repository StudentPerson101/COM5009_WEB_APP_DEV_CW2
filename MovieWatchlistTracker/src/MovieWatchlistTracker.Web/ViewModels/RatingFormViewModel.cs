using System.ComponentModel.DataAnnotations;

namespace MovieWatchlistTracker.Web.ViewModels;

public class RatingFormViewModel
{
    [Required]
    public int MovieId { get; set; }

    [Range(typeof(double), "1", "10")]
    public double Score { get; set; }
}
