using MovieWatchlistTracker.Web.Models;
using MovieWatchlistTracker.Web.ViewModels;

namespace MovieWatchlistTracker.Web.Services.Interfaces;

public interface IAppearanceSettingsService
{
    Task<SiteAppearanceSettings> GetSettingsAsync();

    Task<AdminAppearanceViewModel> GetAdminFormAsync();

    Task<AppearanceUpdateResult> UpdateAsync(AdminAppearanceViewModel model);
}

public sealed record AppearanceUpdateResult(bool Succeeded, string Message)
{
    public static AppearanceUpdateResult Success(string message) => new(true, message);

    public static AppearanceUpdateResult Failure(string message) => new(false, message);
}
