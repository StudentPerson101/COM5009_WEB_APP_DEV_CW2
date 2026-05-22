using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieWatchlistTracker.Web.Data;
using MovieWatchlistTracker.Web.Services.Interfaces;

namespace MovieWatchlistTracker.Web.Controllers;

[Authorize(Roles = SeedData.AdminRoleName)]
public class AdminMaintenanceController : Controller
{
    private readonly ICoverStorageService _coverStorageService;

    public AdminMaintenanceController(ICoverStorageService coverStorageService)
    {
        _coverStorageService = coverStorageService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        return View(await _coverStorageService.GetMaintenanceSummaryAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CleanupUnusedCovers()
    {
        var deletedCount = await _coverStorageService.DeleteUnusedCoversAsync();
        TempData["StatusMessage"] = $"{deletedCount} unused cover file{(deletedCount == 1 ? " was" : "s were")} deleted.";
        return RedirectToAction(nameof(Index));
    }
}
