using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieWatchlistTracker.Web.Data;
using MovieWatchlistTracker.Web.Services.Interfaces;
using MovieWatchlistTracker.Web.ViewModels;

namespace MovieWatchlistTracker.Web.Controllers;

[Authorize(Roles = SeedData.AdminRoleName)]
public class AdminAppearanceController : Controller
{
    private readonly IAppearanceSettingsService _appearanceSettingsService;

    public AdminAppearanceController(IAppearanceSettingsService appearanceSettingsService)
    {
        _appearanceSettingsService = appearanceSettingsService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        return View(await _appearanceSettingsService.GetAdminFormAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(AdminAppearanceViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateCurrentAssetsAsync(model);
            return View(model);
        }

        var result = await _appearanceSettingsService.UpdateAsync(model);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            await PopulateCurrentAssetsAsync(model);
            return View(model);
        }

        TempData["StatusMessage"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateCurrentAssetsAsync(AdminAppearanceViewModel model)
    {
        var current = await _appearanceSettingsService.GetAdminFormAsync();
        model.CurrentLogoPath = current.CurrentLogoPath;
        model.CurrentBannerPath = current.CurrentBannerPath;
    }
}
