using System.ComponentModel.DataAnnotations;

namespace MovieWatchlistTracker.Web.ViewModels;

public class AdminGenreFormViewModel
{
    public int? Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
}
