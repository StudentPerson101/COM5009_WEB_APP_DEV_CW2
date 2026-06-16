using Microsoft.EntityFrameworkCore;
using MovieWatchlistTracker.Web.Data;
using MovieWatchlistTracker.Web.Models;
using MovieWatchlistTracker.Web.Services.Interfaces;
using MovieWatchlistTracker.Web.ViewModels;

namespace MovieWatchlistTracker.Web.Services;

public class AppearanceSettingsService : IAppearanceSettingsService
{
    private const long MaxBrandingBytes = 5 * 1024 * 1024;
    private const string BrandingRequestPath = "/images/branding";
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private static readonly HashSet<string> AllowedExtensionSet = new(AllowedExtensions, StringComparer.OrdinalIgnoreCase);

    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public AppearanceSettingsService(ApplicationDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    public async Task<SiteAppearanceSettings> GetSettingsAsync()
    {
        return await EnsureSettingsAsync();
    }

    public async Task<AdminAppearanceViewModel> GetAdminFormAsync()
    {
        var settings = await EnsureSettingsAsync();

        return new AdminAppearanceViewModel
        {
            CurrentLogoPath = settings.LogoPath,
            CurrentBannerPath = settings.BannerPath,
            PageBackgroundColor = settings.PageBackgroundColor,
            TextColor = settings.TextColor,
            NavigationBarTextColor = settings.NavigationBarTextColor,
            BrowsePageHeadingColor = settings.BrowsePageHeadingColor,
            OutlineColor = settings.OutlineColor,
            OptionButtonOutlineColor = settings.OptionButtonOutlineColor,
            OptionButtonTextColor = settings.OptionButtonTextColor
        };
    }

    public async Task<AppearanceUpdateResult> UpdateAsync(AdminAppearanceViewModel model)
    {
        var settings = await EnsureSettingsAsync();
        string? newLogoPath = null;
        string? newBannerPath = null;

        if (model.LogoUpload is not null)
        {
            var uploadResult = await SaveBrandingImageAsync(model.LogoUpload, "logo");
            if (!uploadResult.Succeeded)
            {
                return AppearanceUpdateResult.Failure(uploadResult.ErrorMessage ?? "The logo image could not be uploaded.");
            }

            newLogoPath = uploadResult.ImageUrl;
        }

        if (model.BannerUpload is not null)
        {
            var uploadResult = await SaveBrandingImageAsync(model.BannerUpload, "banner");
            if (!uploadResult.Succeeded)
            {
                DeleteLocalBrandingFile(newLogoPath);
                return AppearanceUpdateResult.Failure(uploadResult.ErrorMessage ?? "The banner image could not be uploaded.");
            }

            newBannerPath = uploadResult.ImageUrl;
        }

        if (model.RemoveLogo || newLogoPath is not null)
        {
            DeleteLocalBrandingFile(settings.LogoPath);
            settings.LogoPath = newLogoPath;
        }

        if (model.RemoveBanner || newBannerPath is not null)
        {
            DeleteLocalBrandingFile(settings.BannerPath);
            settings.BannerPath = newBannerPath;
        }

        settings.PageBackgroundColor = model.PageBackgroundColor.Trim();
        settings.TextColor = model.TextColor.Trim();
        settings.NavigationBarTextColor = model.NavigationBarTextColor.Trim();
        settings.BrowsePageHeadingColor = model.BrowsePageHeadingColor.Trim();
        settings.OutlineColor = model.OutlineColor.Trim();
        settings.OptionButtonOutlineColor = model.OptionButtonOutlineColor.Trim();
        settings.OptionButtonTextColor = model.OptionButtonTextColor.Trim();

        await _context.SaveChangesAsync();

        return AppearanceUpdateResult.Success("Appearance settings were updated.");
    }

    private async Task<SiteAppearanceSettings> EnsureSettingsAsync()
    {
        var settings = await _context.SiteAppearanceSettings.SingleOrDefaultAsync(settings =>
            settings.Id == SiteAppearanceSettings.SingletonId);

        if (settings is not null)
        {
            return settings;
        }

        settings = SiteAppearanceSettingsDefaults.Create();
        _context.SiteAppearanceSettings.Add(settings);
        await _context.SaveChangesAsync();

        return settings;
    }

    private async Task<BrandingUploadResult> SaveBrandingImageAsync(IFormFile imageFile, string prefix)
    {
        if (imageFile.Length == 0)
        {
            return BrandingUploadResult.Failure("Choose an image before uploading.");
        }

        if (imageFile.Length > MaxBrandingBytes)
        {
            return BrandingUploadResult.Failure("Branding images must be 5 MB or smaller.");
        }

        var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
        if (!AllowedExtensionSet.Contains(extension))
        {
            return BrandingUploadResult.Failure("Branding images must be JPG, PNG, or WebP files.");
        }

        if (!await HasExpectedImageSignatureAsync(imageFile, extension))
        {
            return BrandingUploadResult.Failure("The uploaded image does not match the selected image type.");
        }

        var brandingFolder = GetBrandingFolder();
        Directory.CreateDirectory(brandingFolder);

        var fileName = $"{prefix}-{Guid.NewGuid():N}{extension}";
        var destinationPath = Path.Combine(brandingFolder, fileName);
        await using var destination = new FileStream(destinationPath, FileMode.CreateNew);
        await imageFile.CopyToAsync(destination);

        return BrandingUploadResult.Success($"{BrandingRequestPath}/{fileName}");
    }

    private void DeleteLocalBrandingFile(string? imageUrl)
    {
        var fileName = TryGetLocalBrandingFileName(imageUrl);
        if (fileName is null)
        {
            return;
        }

        if (fileName.StartsWith("default-", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var candidatePath = Path.GetFullPath(Path.Combine(GetBrandingFolder(), fileName));
        if (!IsInsideFolder(candidatePath, GetBrandingFolder()) || !File.Exists(candidatePath))
        {
            return;
        }

        File.Delete(candidatePath);
    }

    private string GetBrandingFolder()
    {
        return Path.GetFullPath(Path.Combine(GetWebRootPath(), "images", "branding"));
    }

    private string GetWebRootPath()
    {
        if (!string.IsNullOrWhiteSpace(_environment.WebRootPath))
        {
            return _environment.WebRootPath;
        }

        return Path.Combine(_environment.ContentRootPath, "wwwroot");
    }

    private static bool IsInsideFolder(string candidatePath, string folder)
    {
        var normalizedFolder = Path.GetFullPath(folder);
        var folderWithSeparator = normalizedFolder.EndsWith(Path.DirectorySeparatorChar)
            ? normalizedFolder
            : normalizedFolder + Path.DirectorySeparatorChar;

        return candidatePath.StartsWith(folderWithSeparator, StringComparison.OrdinalIgnoreCase);
    }

    private static string? TryGetLocalBrandingFileName(string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return null;
        }

        var trimmed = imageUrl.Trim();
        if (Uri.TryCreate(trimmed, UriKind.Absolute, out var absoluteUri) &&
            !absoluteUri.IsFile)
        {
            return null;
        }

        var pathOnly = trimmed
            .Split(['?', '#'], 2)[0]
            .Replace('\\', '/');

        if (!pathOnly.StartsWith(BrandingRequestPath + "/", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var fileName = Path.GetFileName(pathOnly);
        if (string.IsNullOrWhiteSpace(fileName) ||
            !AllowedExtensionSet.Contains(Path.GetExtension(fileName)))
        {
            return null;
        }

        return fileName;
    }

    private static async Task<bool> HasExpectedImageSignatureAsync(IFormFile file, string extension)
    {
        await using var stream = file.OpenReadStream();
        var buffer = new byte[12];
        var read = await stream.ReadAsync(buffer);

        return extension switch
        {
            ".jpg" or ".jpeg" => read >= 3 &&
                buffer[0] == 0xff &&
                buffer[1] == 0xd8 &&
                buffer[2] == 0xff,
            ".png" => read >= 8 &&
                buffer[0] == 0x89 &&
                buffer[1] == 0x50 &&
                buffer[2] == 0x4e &&
                buffer[3] == 0x47 &&
                buffer[4] == 0x0d &&
                buffer[5] == 0x0a &&
                buffer[6] == 0x1a &&
                buffer[7] == 0x0a,
            ".webp" => read >= 12 &&
                buffer[0] == 0x52 &&
                buffer[1] == 0x49 &&
                buffer[2] == 0x46 &&
                buffer[3] == 0x46 &&
                buffer[8] == 0x57 &&
                buffer[9] == 0x45 &&
                buffer[10] == 0x42 &&
                buffer[11] == 0x50,
            _ => false
        };
    }

    private sealed record BrandingUploadResult(bool Succeeded, string? ImageUrl, string? ErrorMessage)
    {
        public static BrandingUploadResult Success(string imageUrl) => new(true, imageUrl, null);

        public static BrandingUploadResult Failure(string errorMessage) => new(false, null, errorMessage);
    }
}
