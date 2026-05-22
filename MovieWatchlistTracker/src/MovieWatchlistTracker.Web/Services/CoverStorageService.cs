using Microsoft.EntityFrameworkCore;
using MovieWatchlistTracker.Web.Data;
using MovieWatchlistTracker.Web.Services.Interfaces;
using MovieWatchlistTracker.Web.ViewModels;

namespace MovieWatchlistTracker.Web.Services;

public class CoverStorageService : ICoverStorageService
{
    private const long MaxCoverBytes = 5 * 1024 * 1024;
    private const string CoverRequestPath = "/images/covers";
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private static readonly HashSet<string> AllowedExtensionSet = new(AllowedExtensions, StringComparer.OrdinalIgnoreCase);

    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public CoverStorageService(ApplicationDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    public async Task<CoverUploadResult> SaveCoverAsync(IFormFile coverFile)
    {
        if (coverFile.Length == 0)
        {
            return CoverUploadResult.Failure("Choose a cover image before uploading.");
        }

        if (coverFile.Length > MaxCoverBytes)
        {
            return CoverUploadResult.Failure("Cover images must be 5 MB or smaller.");
        }

        var extension = Path.GetExtension(coverFile.FileName).ToLowerInvariant();
        if (!AllowedExtensionSet.Contains(extension))
        {
            return CoverUploadResult.Failure("Cover images must be JPG, PNG, or WebP files.");
        }

        if (!await HasExpectedImageSignatureAsync(coverFile, extension))
        {
            return CoverUploadResult.Failure("The uploaded cover does not match the selected image type.");
        }

        var coverFolder = GetCoverFolder();
        Directory.CreateDirectory(coverFolder);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var destinationPath = Path.Combine(coverFolder, fileName);
        await using var destination = new FileStream(destinationPath, FileMode.CreateNew);
        await coverFile.CopyToAsync(destination);

        return CoverUploadResult.Success($"{CoverRequestPath}/{fileName}");
    }

    public async Task<AdminCoverMaintenanceViewModel> GetMaintenanceSummaryAsync()
    {
        var storedFiles = GetStoredCoverFiles();
        var referencedFiles = await GetReferencedCoverFileKeysAsync();

        return new AdminCoverMaintenanceViewModel
        {
            CoverFolderPath = CoverRequestPath,
            TotalCoverFiles = storedFiles.Count,
            InUseCoverFiles = storedFiles.Select(file => file.Key).Intersect(referencedFiles, StringComparer.OrdinalIgnoreCase).Count(),
            UnusedCoverFiles = storedFiles.Select(file => file.Key).Except(referencedFiles, StringComparer.OrdinalIgnoreCase).Count(),
            MissingReferencedFiles = referencedFiles.Except(storedFiles.Select(file => file.Key), StringComparer.OrdinalIgnoreCase).Count()
        };
    }

    public async Task<int> DeleteUnusedCoversAsync()
    {
        var storedFiles = GetStoredCoverFiles();
        var referencedFiles = await GetReferencedCoverFileKeysAsync();
        var deletedCount = 0;

        foreach (var file in storedFiles.Where(file => !referencedFiles.Contains(file.Key)))
        {
            var candidatePath = Path.GetFullPath(file.PhysicalPath);
            if (!IsInsideCoverFolder(candidatePath) || !File.Exists(candidatePath))
            {
                continue;
            }

            File.Delete(candidatePath);
            deletedCount++;
        }

        return deletedCount;
    }

    private IReadOnlyList<StoredCoverFile> GetStoredCoverFiles()
    {
        return EnumerateStoredCoverFiles(CoverRequestPath, GetCoverFolder()).ToArray();
    }

    private IEnumerable<StoredCoverFile> EnumerateStoredCoverFiles(string requestPath, string physicalFolder)
    {
        if (!Directory.Exists(physicalFolder))
        {
            yield break;
        }

        foreach (var filePath in Directory.EnumerateFiles(physicalFolder))
        {
            if (!AllowedExtensionSet.Contains(Path.GetExtension(filePath)))
            {
                continue;
            }

            var fileName = Path.GetFileName(filePath);
            if (string.IsNullOrWhiteSpace(fileName))
            {
                continue;
            }

            yield return new StoredCoverFile($"{requestPath}/{fileName}", filePath);
        }
    }

    private async Task<HashSet<string>> GetReferencedCoverFileKeysAsync()
    {
        var coverUrls = await _context.Movies
            .AsNoTracking()
            .Where(movie => movie.PosterUrl != null)
            .Select(movie => movie.PosterUrl!)
            .ToListAsync();

        return coverUrls
            .Select(TryGetLocalCoverFileKey)
            .Where(fileName => !string.IsNullOrWhiteSpace(fileName))
            .Select(fileName => fileName!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private string GetCoverFolder()
    {
        return Path.GetFullPath(Path.Combine(GetWebRootPath(), "images", "covers"));
    }

    private string GetWebRootPath()
    {
        if (!string.IsNullOrWhiteSpace(_environment.WebRootPath))
        {
            return _environment.WebRootPath;
        }

        return Path.Combine(_environment.ContentRootPath, "wwwroot");
    }

    private bool IsInsideCoverFolder(string candidatePath)
    {
        return IsInsideFolder(candidatePath, GetCoverFolder());
    }

    private static bool IsInsideFolder(string candidatePath, string folder)
    {
        var normalizedFolder = Path.GetFullPath(folder);
        var folderWithSeparator = normalizedFolder.EndsWith(Path.DirectorySeparatorChar)
            ? normalizedFolder
            : normalizedFolder + Path.DirectorySeparatorChar;

        return candidatePath.StartsWith(folderWithSeparator, StringComparison.OrdinalIgnoreCase);
    }

    private static string? TryGetLocalCoverFileKey(string? coverUrl)
    {
        if (string.IsNullOrWhiteSpace(coverUrl))
        {
            return null;
        }

        var trimmed = coverUrl.Trim();
        if (Uri.TryCreate(trimmed, UriKind.Absolute, out var absoluteUri) &&
            !absoluteUri.IsFile)
        {
            return null;
        }

        var pathOnly = trimmed
            .Split(['?', '#'], 2)[0]
            .Replace('\\', '/');

        if (!pathOnly.StartsWith(CoverRequestPath + "/", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var fileName = Path.GetFileName(pathOnly);
        if (string.IsNullOrWhiteSpace(fileName) ||
            !AllowedExtensionSet.Contains(Path.GetExtension(fileName)))
        {
            return null;
        }

        return pathOnly;
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

    private sealed record StoredCoverFile(string Key, string PhysicalPath);
}
