using MovieWatchlistTracker.Web.Models;
using MovieWatchlistTracker.Web.ViewModels;

namespace MovieWatchlistTracker.Web.Services.Interfaces;

public interface IBrowsePageContentService
{
    Task<BrowsePageContentSettings> GetSettingsAsync();
    Task UpdateAsync(BrowsePageHeadingViewModel model);
}
