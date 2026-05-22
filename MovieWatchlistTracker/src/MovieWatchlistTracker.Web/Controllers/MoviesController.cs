using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using MovieWatchlistTracker.Web.Services.Interfaces;

namespace MovieWatchlistTracker.Web.Controllers;

public class MoviesController : Controller
{
    private readonly IMovieSearchService _movieSearchService;

    public MoviesController(IMovieSearchService movieSearchService)
    {
        _movieSearchService = movieSearchService;
    }

    public async Task<IActionResult> Index(
        string? query,
        int? genreId,
        int? year,
        int? minimumRating,
        string? watchedStatus,
        string? sortBy)
    {
        var model = await _movieSearchService.SearchAsync(
            query,
            genreId,
            year,
            minimumRating,
            watchedStatus,
            sortBy,
            CurrentUserIdOrNull());

        return View(model);
    }

    public Task<IActionResult> Search(
        string? query,
        int? genreId,
        int? year,
        int? minimumRating,
        string? watchedStatus,
        string? sortBy)
    {
        return Index(query, genreId, year, minimumRating, watchedStatus, sortBy);
    }

    [HttpGet]
    public async Task<IActionResult> Suggestions(string? query)
    {
        var suggestions = await _movieSearchService.GetTitleSuggestionsAsync(query);
        return Json(suggestions);
    }

    public async Task<IActionResult> Details(int id)
    {
        var model = await _movieSearchService.GetDetailsAsync(id, CurrentUserIdOrNull());
        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    private string? CurrentUserIdOrNull()
    {
        return User.Identity?.IsAuthenticated == true
            ? User.FindFirstValue(ClaimTypes.NameIdentifier)
            : null;
    }
}
