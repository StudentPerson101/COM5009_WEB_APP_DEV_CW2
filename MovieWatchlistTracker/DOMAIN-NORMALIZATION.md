# Domain Model Normalization

## Stage 4 Decision

Use Option B from the Stage 4 specification: keep `Rating` and `Review` as separate domain entities.

This matches the user stories because rating a movie and writing a review are separate actions. A user may rate without reviewing, review without rating, or do both.

## Normalized Entities

| Entity | Normalized role |
| --- | --- |
| `ApplicationUser` | Identity-backed user with navigation collections for watchlists, viewing history, ratings, and reviews |
| `Movie` | Catalog item with many watchlist items, viewing-history items, ratings, reviews, and genre links |
| `Genre` | Unique category linked to movies through `MovieGenre` |
| `MovieGenre` | Join entity for the movie/genre many-to-many relationship |
| `Watchlist` | User-owned list of movies |
| `WatchlistItem` | One movie in one watchlist with an explicit status |
| `ViewingHistoryItem` | Record that a user watched a movie |
| `Rating` | User/movie numeric score, intended range 1 to 5 |
| `Review` | User/movie written review text with timestamps |

## Reconciliations

The class table listed `Movie.genre` as a single field. The normalized model uses `Genre` plus `MovieGenre` because the SQL schema and requirements support multiple genres per movie.

The parts/relationships document says a user has multiple watchlist items. The normalized model routes that through `Watchlist` and `WatchlistItem`, because the SQL schema includes user-owned watchlists and unique `(UserId, Name)` watchlist names.

The SQL schema includes both `reviews.rating` and `ratings.score`. The normalized model removes numeric rating state from `Review`; `Rating.Score` is the authoritative score.

The class table lists `WatchedMovie` or `ViewingHistoryItem`. The normalized model uses `ViewingHistoryItem`.

The SQL schema stores watchlist item status as a checked string. The normalized C# model uses `WatchlistItemStatus` enum values:

- `Planned`
- `Watching`
- `Watched`
- `Dropped`

Future EF Core mapping can convert this enum to the database representation chosen in the migration stage.

## Deferred To Later Stages

- EF Core `DbSet` declarations
- Fluent API constraints and composite keys
- ASP.NET Core Identity registration in `Program.cs`
- SQLite provider configuration
- EF Core migrations
- Seed data
- Service behavior and validation enforcement
