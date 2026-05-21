using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieWatchlistTracker.Web.Services.Interfaces;

namespace MovieWatchlistTracker.Web.Controllers;

[Authorize]
public class HistoryController : Controller
{
    private readonly IViewingHistoryService _viewingHistoryService;

    public HistoryController(IViewingHistoryService viewingHistoryService)
    {
        _viewingHistoryService = viewingHistoryService;
    }

    public async Task<IActionResult> Index(string? sortBy, int? genreId)
    {
        var model = await _viewingHistoryService.GetHistoryAsync(CurrentUserId(), sortBy, genreId);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkWatched(int movieId, string? returnUrl = null)
    {
        try
        {
            await _viewingHistoryService.MarkWatchedAsync(CurrentUserId(), movieId);
            TempData["StatusMessage"] = "Movie marked as watched.";
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }

        return RedirectAfterMutation(returnUrl);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkUnwatched(int movieId, string? returnUrl = null)
    {
        var updated = await _viewingHistoryService.MarkUnwatchedAsync(CurrentUserId(), movieId);
        if (!updated)
        {
            return NotFound();
        }

        TempData["StatusMessage"] = "Movie marked as unwatched.";
        return RedirectAfterMutation(returnUrl);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int movieId, string? returnUrl = null)
    {
        var updated = await _viewingHistoryService.MarkUnwatchedAsync(CurrentUserId(), movieId);
        if (!updated)
        {
            return NotFound();
        }

        TempData["StatusMessage"] = "Movie removed from your watched history.";
        return RedirectAfterMutation(returnUrl);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> WatchAgain(int movieId)
    {
        var updated = await _viewingHistoryService.MarkUnwatchedAsync(CurrentUserId(), movieId);
        if (!updated)
        {
            return NotFound();
        }

        TempData["StatusMessage"] = "Movie returned to your unwatched list.";
        return RedirectToAction("Details", "Movies", new { id = movieId });
    }

    private IActionResult RedirectAfterMutation(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction(nameof(Index));
    }

    private string CurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Authenticated user id is missing.");
    }
}
