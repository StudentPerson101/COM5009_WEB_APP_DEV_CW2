using MovieWatchlistTracker.Web.Services;

namespace MovieWatchlistTracker.Tests;

public class ExternalMovieApiServiceTests
{
    [Fact]
    public void ExternalMovieApiIsDisabledForInternalCatalogStage()
    {
        var service = new ExternalMovieApiService();

        Assert.False(service.IsEnabled);
    }
}
