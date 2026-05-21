using System.ComponentModel.DataAnnotations;

namespace MovieWatchlistTracker.Web.ViewModels;

public class AdminMovieFormViewModel
{
    public int? Id { get; set; }

    [Required]
    [StringLength(255)]
    public string Title { get; set; } = string.Empty;

    [Range(1888, 2100)]
    public int? ReleaseYear { get; set; }

    public string? Description { get; set; }

    public string? PosterUrl { get; set; }

    [Display(Name = "Duration")]
    [Range(1, 1000, ErrorMessage = "Duration must be between 1 and 1000 minutes.")]
    public int? Runtime { get; set; }

    [StringLength(100)]
    public string? ExternalApiId { get; set; }

    public List<int> SelectedGenreIds { get; set; } = [];
    public IReadOnlyList<AdminGenreOptionViewModel> AvailableGenres { get; set; } = [];
}
