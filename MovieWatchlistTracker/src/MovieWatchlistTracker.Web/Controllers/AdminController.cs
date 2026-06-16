using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieWatchlistTracker.Web.Data;

namespace MovieWatchlistTracker.Web.Controllers;

[Authorize(Roles = SeedData.AdminRoleName)]
public class AdminController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
}
