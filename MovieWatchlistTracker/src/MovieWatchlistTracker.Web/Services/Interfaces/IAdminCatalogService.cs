using MovieWatchlistTracker.Web.ViewModels;

namespace MovieWatchlistTracker.Web.Services.Interfaces;

public sealed record AdminCatalogResult(bool Succeeded, string Message, int? EntityId = null);

public interface IAdminCatalogService
{
    Task<AdminMovieIndexViewModel> GetMovieIndexAsync(string? query, int? genreId, string? sortBy);
    Task<AdminMovieDetailsViewModel?> GetMovieDetailsAsync(int movieId);
    Task<AdminMovieFormViewModel> CreateMovieFormAsync();
    Task<AdminMovieFormViewModel?> GetMovieFormAsync(int movieId);
    Task<AdminCatalogResult> CreateMovieAsync(AdminMovieFormViewModel model);
    Task<AdminCatalogResult> UpdateMovieAsync(AdminMovieFormViewModel model);
    Task<AdminCatalogResult> DeleteMovieAsync(int movieId);
    Task<AdminCatalogResult> AssignGenreAsync(int movieId, int genreId);
    Task<AdminCatalogResult> RemoveGenreFromMovieAsync(int movieId, int genreId);

    Task<AdminGenreIndexViewModel> GetGenreIndexAsync();
    Task<AdminGenreFormViewModel?> GetGenreFormAsync(int genreId);
    Task<AdminCatalogResult> CreateGenreAsync(AdminGenreFormViewModel model);
    Task<AdminCatalogResult> UpdateGenreAsync(AdminGenreFormViewModel model);
    Task<AdminCatalogResult> DeleteGenreAsync(int genreId);
}
