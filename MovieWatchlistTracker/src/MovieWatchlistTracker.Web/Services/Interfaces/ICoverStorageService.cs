using Microsoft.AspNetCore.Http;
using MovieWatchlistTracker.Web.ViewModels;

namespace MovieWatchlistTracker.Web.Services.Interfaces;

public interface ICoverStorageService
{
    Task<CoverUploadResult> SaveCoverAsync(IFormFile coverFile);
    Task<AdminCoverMaintenanceViewModel> GetMaintenanceSummaryAsync();
    Task<int> DeleteUnusedCoversAsync();
}
