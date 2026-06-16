using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieWatchlistTracker.Web.Services.Interfaces;
using MovieWatchlistTracker.Web.ViewModels;

namespace MovieWatchlistTracker.Web.Controllers;

[Authorize]
public class RatingsController : Controller
{
    private readonly IRatingService _ratingService;

    public RatingsController(IRatingService ratingService)
    {
        _ratingService = ratingService;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateOrUpdate(RatingFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["StatusMessage"] = "Choose a rating from 1.0 to 10.0.";
            return RedirectToAction("Details", "Movies", new { id = model.MovieId });
        }

        try
        {
            await _ratingService.CreateOrUpdateAsync(CurrentUserId(), model.MovieId, model.Score);
            TempData["StatusMessage"] = "Your rating was saved.";
        }
        catch (ArgumentOutOfRangeException)
        {
            TempData["StatusMessage"] = "Choose a rating from 1.0 to 10.0.";
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }

        return RedirectToAction("Details", "Movies", new { id = model.MovieId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int movieId)
    {
        var deleted = await _ratingService.DeleteAsync(id, CurrentUserId());
        if (!deleted)
        {
            return Forbid();
        }

        TempData["StatusMessage"] = "Your rating was deleted.";
        return RedirectToAction("Details", "Movies", new { id = movieId });
    }

    private string CurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Authenticated user id is missing.");
    }
}
