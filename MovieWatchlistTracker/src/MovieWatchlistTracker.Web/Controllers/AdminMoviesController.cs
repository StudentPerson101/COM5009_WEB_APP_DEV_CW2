using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieWatchlistTracker.Web.Data;
using MovieWatchlistTracker.Web.Services.Interfaces;
using MovieWatchlistTracker.Web.ViewModels;

namespace MovieWatchlistTracker.Web.Controllers;

[Authorize(Roles = SeedData.AdminRoleName)]
public class AdminMoviesController : Controller
{
    private readonly IAdminCatalogService _adminCatalogService;

    public AdminMoviesController(IAdminCatalogService adminCatalogService)
    {
        _adminCatalogService = adminCatalogService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? query, int? genreId, string? sortBy)
    {
        var model = await _adminCatalogService.GetMovieIndexAsync(query, genreId, sortBy);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var model = await _adminCatalogService.GetMovieDetailsAsync(id);
        return model is null ? NotFound() : View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        return View(await _adminCatalogService.CreateMovieFormAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminMovieFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AvailableGenres = (await _adminCatalogService.CreateMovieFormAsync()).AvailableGenres;
            return View(model);
        }

        var result = await _adminCatalogService.CreateMovieAsync(model);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            model.AvailableGenres = (await _adminCatalogService.CreateMovieFormAsync()).AvailableGenres;
            return View(model);
        }

        TempData["StatusMessage"] = result.Message;
        return RedirectToAction(nameof(Details), new { id = result.EntityId });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var model = await _adminCatalogService.GetMovieFormAsync(id);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AdminMovieFormViewModel model)
    {
        if (!model.Id.HasValue)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            model.AvailableGenres = (await _adminCatalogService.CreateMovieFormAsync()).AvailableGenres;
            return View(model);
        }

        var result = await _adminCatalogService.UpdateMovieAsync(model);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            model.AvailableGenres = (await _adminCatalogService.CreateMovieFormAsync()).AvailableGenres;
            return View(model);
        }

        TempData["StatusMessage"] = result.Message;
        return RedirectToAction(nameof(Details), new { id = result.EntityId });
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var model = await _adminCatalogService.GetMovieDetailsAsync(id);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost]
    [ActionName(nameof(Delete))]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var result = await _adminCatalogService.DeleteMovieAsync(id);
        TempData[result.Succeeded ? "StatusMessage" : "ErrorMessage"] = result.Message;

        return result.Succeeded
            ? RedirectToAction(nameof(Index))
            : RedirectToAction(nameof(Delete), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignGenre(int movieId, int genreId)
    {
        var result = await _adminCatalogService.AssignGenreAsync(movieId, genreId);
        TempData[result.Succeeded ? "StatusMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction(nameof(Details), new { id = movieId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveGenreFromMovie(int movieId, int genreId)
    {
        var result = await _adminCatalogService.RemoveGenreFromMovieAsync(movieId, genreId);
        TempData[result.Succeeded ? "StatusMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction(nameof(Details), new { id = movieId });
    }
}
