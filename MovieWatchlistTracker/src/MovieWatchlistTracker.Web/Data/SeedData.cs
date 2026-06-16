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
        new() { Name = "Animated" },
        new() { Name = "Mystery/Thriller" },
        new() { Name = "Buddy-Cop Action Comedy" },
        new() { Name = "Biographical Martial Arts" },
        new() { Name = "Drama" },
        new() { Name = "Family Comedy" },
        new() { Name = "Anime" },
        new() { Name = "Psychological Drama" },
        new() { Name = "Coming-of-Age" },
        new() { Name = "Martial Arts" },
        new() { Name = "Fantasy Adventure" },
        new() { Name = "Dystopian Sci-Fi" },
        new() { Name = "Family" },
        new() { Name = "Romantic Drama" },
        new() { Name = "Romance" },
        new() { Name = "Comedy" },
        new() { Name = "Thriller" },
        new() { Name = "Horror" },
        new() { Name = "History" }
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
        }),
        new("extra-ghost-in-the-shell-2017", new Movie
        {
            Title = "Ghost in the Shell",
            ReleaseYear = 2017,
            Description = "In a cyberpunk future, Major Mira Killian is a cyber-enhanced soldier created to stop dangerous criminals, but her investigation exposes buried truths about her identity and origin.",
            Runtime = 107
        }),
        new("extra-blade-runner-2049-2017", new Movie
        {
            Title = "Blade Runner 2049",
            ReleaseYear = 2017,
            Description = "LAPD blade runner K uncovers a long-hidden secret that could destabilize society and leads him to search for missing former blade runner Rick Deckard.",
            Runtime = 163
        }),
        new("extra-rush-hour-1998", new Movie
        {
            Title = "Rush Hour",
            ReleaseYear = 1998,
            Description = "A disciplined Hong Kong inspector and a loudmouthed LAPD detective are forced to work together to rescue a kidnapped diplomat's daughter.",
            Runtime = 97
        }),
        new("extra-ip-man-2008", new Movie
        {
            Title = "Ip Man",
            ReleaseYear = 2008,
            Description = "A semi-biographical account of Wing Chun master Ip Man, focusing on his life in Foshan during the 1930s-1940s and the Second Sino-Japanese War.",
            Runtime = 108
        }),
        new("extra-the-incredibles-2004", new Movie
        {
            Title = "The Incredibles",
            ReleaseYear = 2004,
            Description = "After superheroes are forced into civilian life, Bob Parr/Mr. Incredible is drawn back into action, putting his family on a path to save him and the world.",
            Runtime = 115
        }),
        new("extra-a-silent-voice-2016", new Movie
        {
            Title = "A Silent Voice",
            ReleaseYear = 2016,
            Description = "A former bully, now isolated and guilt-ridden, tries to reconnect with the deaf girl he tormented in childhood.",
            Runtime = 130
        }),
        new("extra-dragon-ball-super-broly-2018", new Movie
        {
            Title = "Dragon Ball Super: Broly",
            ReleaseYear = 2018,
            Description = "After the Tournament of Power, Goku and Vegeta encounter Broly, a powerful Saiyan whose past ties into the destruction of Planet Vegeta.",
            Runtime = 100
        }),
        new("extra-iron-man-2008", new Movie
        {
            Title = "Iron Man",
            ReleaseYear = 2008,
            Description = "Captured by terrorists, billionaire engineer Tony Stark builds an armored suit to escape, then refines the technology to become Iron Man.",
            Runtime = 126
        }),
        new("extra-total-recall-2012", new Movie
        {
            Title = "Total Recall",
            ReleaseYear = 2012,
            Description = "Factory worker Douglas Quaid visits Rekall for implanted memories, only to discover that his own identity and past may have been manipulated.",
            Runtime = 118
        }),
        new("extra-elysium-2013", new Movie
        {
            Title = "Elysium",
            ReleaseYear = 2013,
            Description = "In 2154, the wealthy live on a luxurious space habitat while the poor remain on ruined Earth; one man takes on a mission that could change both worlds.",
            Runtime = 109
        }),
        new("extra-wolf-children-2012", new Movie
        {
            Title = "Wolf Children",
            ReleaseYear = 2012,
            Description = "After her werewolf partner dies, Hana raises their two half-human, half-wolf children while trying to protect them from society and help them choose their paths.",
            Runtime = 117
        }),
        new("extra-spirited-away-2001", new Movie
        {
            Title = "Spirited Away",
            ReleaseYear = 2001,
            Description = "Ten-year-old Chihiro enters a spirit world after her parents are transformed into pigs, then works in a bathhouse while seeking a way home.",
            Runtime = 125
        }),
        new("extra-eternal-sunshine-of-the-spotless-mind-2004", new Movie
        {
            Title = "Eternal Sunshine of the Spotless Mind",
            ReleaseYear = 2004,
            Description = "After a painful breakup, Joel and Clementine undergo a memory-erasure procedure, but Joel relives their relationship as the memories disappear.",
            Runtime = 108
        }),
        new("extra-humko-deewana-kar-gaye-2006", new Movie
        {
            Title = "Humko Deewana Kar Gaye",
            ReleaseYear = 2006,
            Description = "Aditya, an engineer travelling to Canada for training, meets Jia, a woman from a wealthy family. Although both are committed to other people, repeated encounters draw them into a complicated romance.",
            Runtime = 152
        }),
        new("extra-singh-is-kinng-2008", new Movie
        {
            Title = "Singh is Kinng",
            ReleaseYear = 2008,
            Description = "Happy Singh, a well-meaning Punjabi villager, is sent to bring back a gangster from Australia but stumbles through misadventures that eventually make him the king of the underworld.",
            Runtime = 136
        }),
        new("extra-baaghi-2016", new Movie
        {
            Title = "Baaghi",
            ReleaseYear = 2016,
            Description = "Ronny, a rebellious martial-arts student, falls in love with Sia. When she is later abducted by a martial-arts champion, Ronny must fight to rescue her.",
            Runtime = 133
        }),
        new("extra-stree-2018", new Movie
        {
            Title = "Stree",
            ReleaseYear = 2018,
            Description = "In the town of Chanderi, men fear a mysterious female spirit called Stree, who appears at night and abducts men, leaving only their clothes behind.",
            Runtime = 127
        }),
        new("extra-chhaava-2025", new Movie
        {
            Title = "Chhaava",
            ReleaseYear = 2025,
            Description = "After Chhatrapati Shivaji Maharaj's death, Sambhaji Maharaj leads the Maratha resistance against Mughal forces under Aurangzeb in a struggle for power and survival.",
            Runtime = 162
        })
    ];

    public static IReadOnlyDictionary<string, string[]> MovieGenres => new Dictionary<string, string[]>
    {
        ["demo-avengers-age-of-ultron"] = ["Superhero", "Action", "Sci-Fi"],
        ["demo-wall-e"] = ["Animated", "Romantic", "Sci-Fi", "Adventure"],
        ["demo-man-of-steel"] = ["Superhero", "Action", "Sci-Fi"],
        ["demo-alita-battle-angel"] = ["Sci-Fi", "Action", "Adventure", "Fantasy", "Animation"],
        ["extra-ghost-in-the-shell-2017"] = ["Sci-Fi", "Action", "Adventure"],
        ["extra-blade-runner-2049-2017"] = ["Sci-Fi", "Mystery/Thriller", "Action"],
        ["extra-rush-hour-1998"] = ["Buddy-Cop Action Comedy"],
        ["extra-ip-man-2008"] = ["Biographical Martial Arts", "Action", "Drama"],
        ["extra-the-incredibles-2004"] = ["Animated", "Superhero", "Action", "Family Comedy"],
        ["extra-a-silent-voice-2016"] = ["Anime", "Psychological Drama", "Coming-of-Age"],
        ["extra-dragon-ball-super-broly-2018"] = ["Anime", "Martial Arts", "Fantasy Adventure", "Action"],
        ["extra-iron-man-2008"] = ["Superhero", "Sci-Fi", "Action"],
        ["extra-total-recall-2012"] = ["Sci-Fi", "Action", "Adventure", "Mystery/Thriller"],
        ["extra-elysium-2013"] = ["Dystopian Sci-Fi", "Action"],
        ["extra-wolf-children-2012"] = ["Anime", "Fantasy", "Drama", "Family"],
        ["extra-spirited-away-2001"] = ["Anime", "Fantasy", "Adventure"],
        ["extra-eternal-sunshine-of-the-spotless-mind-2004"] = ["Sci-Fi", "Romantic Drama", "Psychological Drama"],
        ["extra-humko-deewana-kar-gaye-2006"] = ["Romance", "Drama"],
        ["extra-singh-is-kinng-2008"] = ["Action", "Comedy"],
        ["extra-baaghi-2016"] = ["Action", "Romance", "Thriller"],
        ["extra-stree-2018"] = ["Horror", "Comedy"],
        ["extra-chhaava-2025"] = ["Action", "History", "Drama"]
    };

    public static IReadOnlyDictionary<string, WatchlistItemStatus> WatchlistStatuses => new Dictionary<string, WatchlistItemStatus>
    {
        ["demo-avengers-age-of-ultron"] = WatchlistItemStatus.Watched,
        ["demo-wall-e"] = WatchlistItemStatus.Planned,
        ["demo-man-of-steel"] = WatchlistItemStatus.Watched,
        ["demo-alita-battle-angel"] = WatchlistItemStatus.Watching
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
