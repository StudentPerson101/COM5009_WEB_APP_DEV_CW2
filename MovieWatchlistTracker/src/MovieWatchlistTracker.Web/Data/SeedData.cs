using MovieWatchlistTracker.Web.Models;

namespace MovieWatchlistTracker.Web.Data;

public sealed record SeedMovieDefinition(string CatalogKey, Movie Movie);

public static class SeedData
{
    public const string SampleUserId = "dev-user-0001";
    public const string SampleUserName = "dev.user";
    public const string SampleUserEmail = "dev.user@example.local";
    public const string SampleUserPassword = "DevUser!234";
    public const string AdminRoleId = "role-admin";
    public const string AdminRoleName = "Admin";
    public const string DefaultWatchlistName = "My Watchlist";

    public static readonly DateTime SeededAt = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    public static IReadOnlyList<Genre> Genres =>
    [
        new() { Name = "Superhero" },
        new() { Name = "Action" },
        new() { Name = "Sci-Fi" },
        new() { Name = "Romantic" },
        new() { Name = "Adventure" },
        new() { Name = "Fantasy" },
        new() { Name = "Animation" },
        new() { Name = "Animated" }
    ];

    public static IReadOnlyList<SeedMovieDefinition> Movies =>
    [
        new("demo-avengers-age-of-ultron", new Movie
        {
            Title = "Avengers: Age of Ultron",
            ReleaseYear = 2015,
            Description = "Tony Stark's attempt to activate a peacekeeping program creates Ultron, an AI threat bent on human extinction, forcing the Avengers to reunite and face new powerful adversaries.",
            Runtime = 141
        }),
        new("demo-wall-e", new Movie
        {
            Title = "WALL-E",
            ReleaseYear = 2008,
            Description = "WALL-E, the last trash-compacting robot on an abandoned Earth, develops a personality after centuries alone and follows the probe EVE on a galaxy-spanning adventure.",
            Runtime = 98
        }),
        new("demo-man-of-steel", new Movie
        {
            Title = "Man of Steel",
            ReleaseYear = 2013,
            Description = "Clark Kent, sent to Earth from Krypton as an infant, must embrace his identity as Superman when survivors from his home planet threaten Earth.",
            Runtime = 143
        }),
        new("demo-alita-battle-angel", new Movie
        {
            Title = "Alita: Battle Angel",
            ReleaseYear = 2019,
            Description = "In a future Iron City, a compassionate cyber-doctor rebuilds an amnesiac cyborg named Alita, who begins uncovering her identity and dangerous past.",
            Runtime = 122
        })
    ];

    public static IReadOnlyDictionary<string, string[]> MovieGenres => new Dictionary<string, string[]>
    {
        ["demo-avengers-age-of-ultron"] = ["Superhero", "Action", "Sci-Fi"],
        ["demo-wall-e"] = ["Animated", "Romantic", "Sci-Fi", "Adventure"],
        ["demo-man-of-steel"] = ["Superhero", "Action", "Sci-Fi"],
        ["demo-alita-battle-angel"] = ["Sci-Fi", "Action", "Adventure", "Fantasy", "Animation"]
    };

    public static IReadOnlyDictionary<string, WatchlistItemStatus> WatchlistStatuses => new Dictionary<string, WatchlistItemStatus>
    {
        ["demo-avengers-age-of-ultron"] = WatchlistItemStatus.Watched,
        ["demo-wall-e"] = WatchlistItemStatus.Planned,
        ["demo-man-of-steel"] = WatchlistItemStatus.Watched,
        ["demo-alita-battle-angel"] = WatchlistItemStatus.Watching
    };

    public static IReadOnlyDictionary<string, int> Ratings => new Dictionary<string, int>
    {
        ["demo-avengers-age-of-ultron"] = 4,
        ["demo-wall-e"] = 5,
        ["demo-man-of-steel"] = 3,
        ["demo-alita-battle-angel"] = 4
    };

    public static IReadOnlyDictionary<string, string> Reviews => new Dictionary<string, string>
    {
        ["demo-avengers-age-of-ultron"] = "Big, busy, and full of team-up spectacle.",
        ["demo-wall-e"] = "Tender, funny, and quietly enormous in scope.",
        ["demo-man-of-steel"] = "A weighty superhero origin with huge-scale action.",
        ["demo-alita-battle-angel"] = "Kinetic sci-fi adventure with a memorable central hero."
    };

    public static IReadOnlyList<(string Title, int ReleaseYear)> LegacyDemoMovies =>
    [
        ("The Silent Orbit", 2024),
        ("Midnight Cartographers", 2022),
        ("Summer At The Bijou", 2021),
        ("Neon Harbor", 2023),
        ("The Last Paper Dragon", 2020),
        ("Northbound Platform", 2019)
    ];

    public static IReadOnlyList<string> LegacyDemoGenres =>
    [
        "Comedy",
        "Drama",
        "Science Fiction",
        "Thriller"
    ];
}
