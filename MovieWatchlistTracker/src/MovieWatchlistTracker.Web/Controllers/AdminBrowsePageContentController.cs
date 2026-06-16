using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieWatchlistTracker.Web.Data;
using MovieWatchlistTracker.Web.Services.Interfaces;
using MovieWatchlistTracker.Web.ViewModels;

namespace MovieWatchlistTracker.Web.Controllers;

[Authorize(Roles = SeedData.AdminRoleName)]
public class AdminBrowsePageContentController : Controller
{
    private readonly IBrowsePageContentService _browsePageContentService;

    public AdminBrowsePageContentController(IBrowsePageContentService browsePageContentService)
    {
        _browsePageContentService = browsePageContentService;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(BrowsePageHeadingViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Browse page headings could not be updated. Check the heading text and try again.";
            return RedirectToLocalBrowse(model.ReturnUrl);
        }

        await _browsePageContentService.UpdateAsync(model);
        TempData["StatusMessage"] = "Browse page headings were updated.";
        return RedirectToLocalBrowse(model.ReturnUrl);
    }

    private IActionResult RedirectToLocalBrowse(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction("Index", "Movies");
    }
}
