using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using MovieWatchlistTracker.Tests.TestData;
using MovieWatchlistTracker.Web.Services;

namespace MovieWatchlistTracker.Tests;

public class CoverStorageServiceTests
{
    [Fact]
    public async Task GetMaintenanceSummaryAsync_CountsOnlyLocalCoverFolderFiles()
    {
        using var fixture = new CoverStorageFixture();
        using var database = new TestDatabase();
        await using var context = database.CreateContext();
        await SetCoverUrlsAsync(context, "/images/covers/in-use.jpg", "/images/covers/missing.webp");
        fixture.WriteCover("in-use.jpg", [0xff, 0xd8, 0xff]);
        fixture.WriteCover("unused.png", [0x89, 0x50, 0x4e, 0x47]);
        fixture.WriteCover("unused.webp", [0x52, 0x49, 0x46, 0x46, 0, 0, 0, 0, 0x57, 0x45, 0x42, 0x50]);
        fixture.WriteWebRootFile("unrelated.jpg", [0xff, 0xd8, 0xff]);

        var service = new CoverStorageService(context, fixture.Environment);

        var summary = await service.GetMaintenanceSummaryAsync();

        Assert.Equal(3, summary.TotalCoverFiles);
        Assert.Equal(1, summary.InUseCoverFiles);
        Assert.Equal(2, summary.UnusedCoverFiles);
        Assert.Equal(1, summary.MissingReferencedFiles);
        Assert.Equal("/images/covers", summary.CoverFolderPath);
    }

    [Fact]
    public async Task DeleteUnusedCoversAsync_DeletesOnlyUnreferencedCoverImages()
    {
        using var fixture = new CoverStorageFixture();
        using var database = new TestDatabase();
        await using var context = database.CreateContext();
        await SetCoverUrlsAsync(context, "/images/covers/keep.jpg", null);
        fixture.WriteCover("keep.jpg", [0xff, 0xd8, 0xff]);
        fixture.WriteCover("delete.png", [0x89, 0x50, 0x4e, 0x47]);
        fixture.WriteCover("notes.txt", [0x01, 0x02]);
        fixture.WriteWebRootFile("outside-covers.png", [0x89, 0x50, 0x4e, 0x47]);

        var service = new CoverStorageService(context, fixture.Environment);

        var deletedCount = await service.DeleteUnusedCoversAsync();

        Assert.Equal(1, deletedCount);
        Assert.True(File.Exists(fixture.CoverPath("keep.jpg")));
        Assert.False(File.Exists(fixture.CoverPath("delete.png")));
        Assert.True(File.Exists(fixture.CoverPath("notes.txt")));
        Assert.True(File.Exists(Path.Combine(fixture.WebRootPath, "outside-covers.png")));
    }

    [Fact]
    public async Task SaveCoverAsync_PreservesOriginalAllowedImageType()
    {
        using var fixture = new CoverStorageFixture();
        using var database = new TestDatabase();
        await using var context = database.CreateContext();
        var service = new CoverStorageService(context, fixture.Environment);
        var bytes = new byte[] { 0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a, 0x00 };
        await using var stream = new MemoryStream(bytes);
        var upload = new FormFile(stream, 0, bytes.Length, "CoverUpload", "cover.PNG")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/png"
        };

        var result = await service.SaveCoverAsync(upload);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.CoverUrl);
        Assert.StartsWith("/images/covers/", result.CoverUrl);
        Assert.EndsWith(".png", result.CoverUrl);
        Assert.True(File.Exists(fixture.CoverPath(Path.GetFileName(result.CoverUrl)!)));
    }

    [Fact]
    public async Task SaveCoverAsync_RejectsMismatchedImageSignature()
    {
        using var fixture = new CoverStorageFixture();
        using var database = new TestDatabase();
        await using var context = database.CreateContext();
        var service = new CoverStorageService(context, fixture.Environment);
        var bytes = new byte[] { 0x89, 0x50, 0x4e, 0x47 };
        await using var stream = new MemoryStream(bytes);
        var upload = new FormFile(stream, 0, bytes.Length, "CoverUpload", "cover.jpg");

        var result = await service.SaveCoverAsync(upload);

        Assert.False(result.Succeeded);
        Assert.Contains("does not match", result.ErrorMessage);
        Assert.Empty(Directory.EnumerateFiles(fixture.CoverFolderPath));
    }

    private static async Task SetCoverUrlsAsync(
        MovieWatchlistTracker.Web.Data.ApplicationDbContext context,
        string? firstCoverUrl,
        string? secondCoverUrl)
    {
        var movies = await context.Movies
            .OrderBy(movie => movie.Title)
            .ToArrayAsync();

        movies[0].PosterUrl = firstCoverUrl;
        movies[1].PosterUrl = secondCoverUrl;
        await context.SaveChangesAsync();
    }

    private sealed class CoverStorageFixture : IDisposable
    {
        private readonly string _rootPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "TestArtifacts",
            Guid.NewGuid().ToString("N"));

        public CoverStorageFixture()
        {
            WebRootPath = Path.Combine(_rootPath, "wwwroot");
            CoverFolderPath = Path.Combine(WebRootPath, "images", "covers");
            Directory.CreateDirectory(CoverFolderPath);
            Environment = new TestWebHostEnvironment(_rootPath, WebRootPath);
        }

        public string WebRootPath { get; }
        public string CoverFolderPath { get; }
        public IWebHostEnvironment Environment { get; }

        public string CoverPath(string fileName)
        {
            return Path.Combine(CoverFolderPath, fileName);
        }

        public void WriteCover(string fileName, byte[] bytes)
        {
            File.WriteAllBytes(CoverPath(fileName), bytes);
        }

        public void WriteWebRootFile(string fileName, byte[] bytes)
        {
            File.WriteAllBytes(Path.Combine(WebRootPath, fileName), bytes);
        }

        public void Dispose()
        {
            if (Directory.Exists(_rootPath))
            {
                Directory.Delete(_rootPath, recursive: true);
            }
        }
    }

    private sealed class TestWebHostEnvironment : IWebHostEnvironment
    {
        public TestWebHostEnvironment(string contentRootPath, string webRootPath)
        {
            ContentRootPath = contentRootPath;
            WebRootPath = webRootPath;
            ContentRootFileProvider = new NullFileProvider();
            WebRootFileProvider = new NullFileProvider();
        }

        public string ApplicationName { get; set; } = "MovieWatchlistTracker.Tests";
        public IFileProvider ContentRootFileProvider { get; set; }
        public string ContentRootPath { get; set; }
        public string EnvironmentName { get; set; } = "Development";
        public IFileProvider WebRootFileProvider { get; set; }
        public string WebRootPath { get; set; }
    }
}
