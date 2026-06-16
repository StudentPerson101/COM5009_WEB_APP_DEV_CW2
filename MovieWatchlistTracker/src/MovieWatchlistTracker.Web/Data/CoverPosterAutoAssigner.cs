using System.Text.RegularExpressions;
using MovieWatchlistTracker.Web.Models;

namespace MovieWatchlistTracker.Web.Data;

public static class CoverPosterAutoAssigner
{
    private const string BackupCoverFolderName = "movie cover posters";
    private const string CoverRequestPath = "/images/covers";
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private static readonly HashSet<string> AllowedExtensionSet = new(AllowedExtensions, StringComparer.OrdinalIgnoreCase);

    public static void AssignMissingCovers(ApplicationDbContext context, IWebHostEnvironment environment)
    {
        var backupCoverFolder = FindBackupCoverFolder(environment.ContentRootPath);
        if (backupCoverFolder is null)
        {
            return;
        }

        var coverFolder = GetCoverFolder(environment);
        Directory.CreateDirectory(coverFolder);

        var coverCandidates = GetCoverCandidates(backupCoverFolder);
        if (coverCandidates.Count == 0)
        {
            return;
        }

        var changed = false;
        foreach (var movie in context.Movies.ToList())
        {
            if (!NeedsLocalCover(movie, coverFolder))
            {
                continue;
            }

            var normalizedTitle = NormalizeTitle(movie.Title);
            if (!coverCandidates.TryGetValue(normalizedTitle, out var candidate))
            {
                continue;
            }

            var targetFileName = TryGetLocalCoverFileName(movie.PosterUrl) ??
                BuildCoverFileName(movie, candidate.Extension);
            var targetPath = Path.Combine(coverFolder, targetFileName);

            if (!File.Exists(targetPath) || new FileInfo(targetPath).Length == 0)
            {
                File.Copy(candidate.Path, targetPath, overwrite: true);
            }

            movie.PosterUrl = $"{CoverRequestPath}/{targetFileName}";
            changed = true;
        }

        if (changed)
        {
            context.SaveChanges();
        }
    }

    private static Dictionary<string, CoverCandidate> GetCoverCandidates(string backupCoverFolder)
    {
        return Directory.EnumerateFiles(backupCoverFolder)
            .Where(filePath => AllowedExtensionSet.Contains(Path.GetExtension(filePath)))
            .Select(CreateCoverCandidate)
            .GroupBy(candidate => candidate.NormalizedTitle, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderByDescending(candidate => candidate.CopyNumber)
                    .ThenByDescending(candidate => candidate.Length)
                    .First(),
                StringComparer.OrdinalIgnoreCase);
    }

    private static CoverCandidate CreateCoverCandidate(string filePath)
    {
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
        var copyNumber = TryGetTrailingNumber(fileNameWithoutExtension) ?? 0;
        var title = StripTrailingNumber(fileNameWithoutExtension);
        title = StripTrailingYear(title);

        return new CoverCandidate(
            filePath,
            NormalizeTitle(title),
            Path.GetExtension(filePath).ToLowerInvariant(),
            copyNumber,
            new FileInfo(filePath).Length);
    }

    private static bool NeedsLocalCover(Movie movie, string coverFolder)
    {
        if (string.IsNullOrWhiteSpace(movie.PosterUrl))
        {
            return true;
        }

        var fileName = TryGetLocalCoverFileName(movie.PosterUrl);
        if (fileName is null)
        {
            return false;
        }

        var coverPath = Path.Combine(coverFolder, fileName);
        return !File.Exists(coverPath) || new FileInfo(coverPath).Length == 0;
    }

    private static string? FindBackupCoverFolder(string contentRootPath)
    {
        var candidateFolders = new[]
        {
            Path.Combine(contentRootPath, BackupCoverFolderName),
            Path.Combine(contentRootPath, "..", BackupCoverFolderName),
            Path.Combine(contentRootPath, "..", "..", BackupCoverFolderName),
            Path.Combine(Directory.GetCurrentDirectory(), BackupCoverFolderName)
        };

        return candidateFolders
            .Select(Path.GetFullPath)
            .FirstOrDefault(Directory.Exists);
    }

    private static string GetCoverFolder(IWebHostEnvironment environment)
    {
        var webRootPath = string.IsNullOrWhiteSpace(environment.WebRootPath)
            ? Path.Combine(environment.ContentRootPath, "wwwroot")
            : environment.WebRootPath;

        return Path.GetFullPath(Path.Combine(webRootPath, "images", "covers"));
    }

    private static string? TryGetLocalCoverFileName(string? posterUrl)
    {
        if (string.IsNullOrWhiteSpace(posterUrl))
        {
            return null;
        }

        var trimmed = posterUrl.Trim();
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

        return fileName;
    }

    private static string BuildCoverFileName(Movie movie, string extension)
    {
        var yearOrId = movie.ReleaseYear?.ToString() ?? $"movie-{movie.Id}";
        return $"{Slugify(movie.Title)}-{yearOrId}{extension}";
    }

    private static string NormalizeTitle(string title)
    {
        return Regex.Replace(title.ToLowerInvariant(), "[^a-z0-9]", string.Empty);
    }

    private static string Slugify(string title)
    {
        var slug = Regex.Replace(title.ToLowerInvariant(), "[^a-z0-9]+", "-").Trim('-');
        return string.IsNullOrWhiteSpace(slug) ? "movie-cover" : slug;
    }

    private static int? TryGetTrailingNumber(string value)
    {
        var match = Regex.Match(value, @"\((\d+)\)\s*$");
        return match.Success && int.TryParse(match.Groups[1].Value, out var number)
            ? number
            : null;
    }

    private static string StripTrailingNumber(string value)
    {
        return Regex.Replace(value, @"\s*\(\d+\)\s*$", string.Empty);
    }

    private static string StripTrailingYear(string value)
    {
        return Regex.Replace(value, @"\s*\((?:19|20)\d{2}\)\s*$", string.Empty);
    }

    private sealed record CoverCandidate(
        string Path,
        string NormalizedTitle,
        string Extension,
        int CopyNumber,
        long Length);
}
