using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using MovieWatchlistTracker.Web.Models;
using MovieWatchlistTracker.Web.Services.Interfaces;

namespace MovieWatchlistTracker.Web.Controllers;

public class HomeController : Controller
{
    private readonly IMovieSearchService _movieSearchService;

    public HomeController(IMovieSearchService movieSearchService)
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

        return View("~/Views/Movies/Index.cshtml", model);
    }

    public IActionResult Profile()
    {
        return RedirectToAction("Index", "Profile");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private string? CurrentUserIdOrNull()
    {
        return User.Identity?.IsAuthenticated == true
            ? User.FindFirstValue(ClaimTypes.NameIdentifier)
            : null;
    }
}
