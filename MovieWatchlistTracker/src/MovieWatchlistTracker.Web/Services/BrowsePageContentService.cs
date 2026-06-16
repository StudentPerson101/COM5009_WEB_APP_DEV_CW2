using Microsoft.EntityFrameworkCore;
using MovieWatchlistTracker.Web.Data;
using MovieWatchlistTracker.Web.Models;
using MovieWatchlistTracker.Web.Services.Interfaces;
using MovieWatchlistTracker.Web.ViewModels;

namespace MovieWatchlistTracker.Web.Services;

public class BrowsePageContentService : IBrowsePageContentService
{
    private readonly ApplicationDbContext _context;

    public BrowsePageContentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<BrowsePageContentSettings> GetSettingsAsync()
    {
        return await EnsureSettingsAsync();
    }

    public async Task UpdateAsync(BrowsePageHeadingViewModel model)
    {
        var settings = await EnsureSettingsAsync();
        settings.EyebrowText = model.EyebrowText.Trim();
        settings.HeadingText = model.HeadingText.Trim();

        await _context.SaveChangesAsync();
    }

    private async Task<BrowsePageContentSettings> EnsureSettingsAsync()
    {
        var settings = await _context.BrowsePageContentSettings.SingleOrDefaultAsync(settings =>
            settings.Id == BrowsePageContentSettings.SingletonId);

        if (settings is not null)
        {
            return settings;
        }

        settings = BrowsePageContentSettingsDefaults.Create();
        _context.BrowsePageContentSettings.Add(settings);
        await _context.SaveChangesAsync();

        return settings;
    }
}
