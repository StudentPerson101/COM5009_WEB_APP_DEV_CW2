using MovieWatchlistTracker.Web.Models;

namespace MovieWatchlistTracker.Web.Services.Interfaces;

public interface IGenreService
{
    Task<IReadOnlyList<Genre>> GetAllAsync();
}
