using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieWatchlistTracker.Web.Data;
using MovieWatchlistTracker.Web.Services.Interfaces;
using MovieWatchlistTracker.Web.ViewModels;

namespace MovieWatchlistTracker.Web.Controllers;

[Authorize(Roles = SeedData.AdminRoleName)]
public class AdminGenresController : Controller
{
    private readonly IAdminCatalogService _adminCatalogService;

    public AdminGenresController(IAdminCatalogService adminCatalogService)
    {
        _adminCatalogService = adminCatalogService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        return View(await _adminCatalogService.GetGenreIndexAsync());
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new AdminGenreFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminGenreFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _adminCatalogService.CreateGenreAsync(model);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }

        TempData["StatusMessage"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var model = await _adminCatalogService.GetGenreFormAsync(id);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AdminGenreFormViewModel model)
    {
        if (!model.Id.HasValue)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _adminCatalogService.UpdateGenreAsync(model);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }

        TempData["StatusMessage"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var model = (await _adminCatalogService.GetGenreIndexAsync())
            .Genres
            .SingleOrDefault(genre => genre.Id == id);

        return model is null ? NotFound() : View(model);
    }

    [HttpPost]
    [ActionName(nameof(Delete))]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var result = await _adminCatalogService.DeleteGenreAsync(id);
        TempData[result.Succeeded ? "StatusMessage" : "ErrorMessage"] = result.Message;

        return result.Succeeded
            ? RedirectToAction(nameof(Index))
            : RedirectToAction(nameof(Delete), new { id });
    }
}
