namespace MovieWatchlistTracker.Web.ViewModels;

public sealed record CoverUploadResult(bool Succeeded, string? CoverUrl, string? ErrorMessage)
{
    public static CoverUploadResult Success(string coverUrl)
    {
        return new CoverUploadResult(true, coverUrl, null);
    }

    public static CoverUploadResult Failure(string errorMessage)
    {
        return new CoverUploadResult(false, null, errorMessage);
    }
}
