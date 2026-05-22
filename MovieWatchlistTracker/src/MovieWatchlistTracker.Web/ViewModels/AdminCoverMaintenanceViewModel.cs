namespace MovieWatchlistTracker.Web.ViewModels;

public class AdminCoverMaintenanceViewModel
{
    public int TotalCoverFiles { get; set; }
    public int InUseCoverFiles { get; set; }
    public int UnusedCoverFiles { get; set; }
    public int MissingReferencedFiles { get; set; }
    public string CoverFolderPath { get; set; } = "/images/covers";
}
