using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieWatchlistTracker.Web.Services.Interfaces;

namespace MovieWatchlistTracker.Web.Controllers;

[Authorize]
public class WatchlistsController : Controller
{
    private readonly IWatchlistService _watchlistService;

    public WatchlistsController(IWatchlistService watchlistService)
    {
        _watchlistService = watchlistService;
    }

    public async Task<IActionResult> Index(
        string? query,
        string? sortBy,
        int? genreId,
        int? year,
        int? minimumRating,
        string? status)
    {
        var model = await _watchlistService.GetUserWatchlistAsync(
            CurrentUserId(),
            query,
            sortBy,
            genreId,
            year,
            minimumRating,
            status);

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Suggestions(string? query)
    {
        var suggestions = await _watchlistService.GetTitleSuggestionsAsync(CurrentUserId(), query);
        return Json(suggestions);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddMovie(int movieId)
    {
        if (await _watchlistService.IsMovieInUserWatchlistAsync(CurrentUserId(), movieId))
        {
            TempData["StatusMessage"] = "Movie is already in your watchlist.";
            return RedirectToAction("Details", "Movies", new { id = movieId });
        }

        try
        {
            await _watchlistService.AddMovieAsync(CurrentUserId(), movieId);
            TempData["StatusMessage"] = "Movie added to your watchlist.";
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }

        return RedirectToAction("Details", "Movies", new { id = movieId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveMovie(int movieId, string? returnUrl = null)
    {
        var removed = await _watchlistService.RemoveMovieAsync(CurrentUserId(), movieId);
        if (!removed)
        {
            return Forbid();
        }

        TempData["StatusMessage"] = "Movie removed from your watchlist.";
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveItem(int id)
    {
        var removed = await _watchlistService.RemoveItemAsync(id, CurrentUserId());
        if (!removed)
        {
            return Forbid();
        }

        TempData["StatusMessage"] = "Movie removed from your watchlist.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkWatched(int id)
    {
        var updated = await _watchlistService.MarkWatchedAsync(id, CurrentUserId());
        if (!updated)
        {
            return Forbid();
        }

        TempData["StatusMessage"] = "Movie marked as watched.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkUnwatched(int id)
    {
        var updated = await _watchlistService.MarkUnwatchedAsync(id, CurrentUserId());
        if (!updated)
        {
            return Forbid();
        }

        TempData["StatusMessage"] = "Movie marked as unwatched.";
        return RedirectToAction(nameof(Index));
    }

    private string CurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Authenticated user id is missing.");
    }
}
