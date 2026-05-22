using MovieWatchlistTracker.Web.Services.Interfaces;

namespace MovieWatchlistTracker.Web.Services;

public class ExternalMovieApiService : IExternalMovieApiService
{
    public bool IsEnabled => false;
}
