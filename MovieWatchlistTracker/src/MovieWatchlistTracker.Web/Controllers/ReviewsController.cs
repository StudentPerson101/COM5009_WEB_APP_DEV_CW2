using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieWatchlistTracker.Web.Services.Interfaces;
using MovieWatchlistTracker.Web.ViewModels;

namespace MovieWatchlistTracker.Web.Controllers;

[Authorize]
public class ReviewsController : Controller
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ReviewFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["StatusMessage"] = "Review text is required and must be under 4000 characters.";
            return RedirectToAction("Details", "Movies", new { id = model.MovieId });
        }

        try
        {
            await _reviewService.CreateOrUpdateForMovieAsync(CurrentUserId(), model.MovieId, model.Text);
            TempData["StatusMessage"] = "Your review was saved.";
        }
        catch (ArgumentException)
        {
            TempData["StatusMessage"] = "Review text is required and must be under 4000 characters.";
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }

        return RedirectToAction("Details", "Movies", new { id = model.MovieId });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var review = await _reviewService.GetReviewForUserAsync(id, CurrentUserId());
        if (review is null)
        {
            return await _reviewService.ReviewExistsAsync(id) ? Forbid() : NotFound();
        }

        return View(new ReviewEditViewModel
        {
            Id = review.Id,
            MovieId = review.MovieId,
            Text = review.Text
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ReviewEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        bool updated;
        try
        {
            updated = await _reviewService.UpdateAsync(model.Id, CurrentUserId(), model.Text);
        }
        catch (ArgumentException)
        {
            ModelState.AddModelError(nameof(model.Text), "Review text is required and must be under 4000 characters.");
            return View(model);
        }

        if (!updated)
        {
            return await _reviewService.ReviewExistsAsync(model.Id) ? Forbid() : NotFound();
        }

        TempData["StatusMessage"] = "Your review was updated.";
        return RedirectToAction("Details", "Movies", new { id = model.MovieId });
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var review = await _reviewService.GetReviewForUserAsync(id, CurrentUserId());
        if (review is null)
        {
            return await _reviewService.ReviewExistsAsync(id) ? Forbid() : NotFound();
        }

        return View(new ReviewEditViewModel
        {
            Id = review.Id,
            MovieId = review.MovieId,
            Text = review.Text
        });
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, int movieId)
    {
        var deleted = await _reviewService.DeleteAsync(id, CurrentUserId());
        if (!deleted)
        {
            return await _reviewService.ReviewExistsAsync(id) ? Forbid() : NotFound();
        }

        TempData["StatusMessage"] = "Your review was deleted.";
        return RedirectToAction("Details", "Movies", new { id = movieId });
    }

    private string CurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Authenticated user id is missing.");
    }
}
