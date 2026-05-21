# MovieWatchlistTracker SPEC

## 1. Project Summary

MovieWatchlistTracker is a local ASP.NET Core MVC web app for browsing a movie catalog, saving movies to a personal watchlist, marking movies as watched, rating movies, writing reviews, and viewing watched history.

The first implementation should be a focused MVP using an internally seeded movie catalog. External movie API integration is optional and deferred.

## 2. Stage 1 Decisions

| Area | Decision |
| --- | --- |
| Web framework | ASP.NET Core MVC |
| Language | C# |
| UI | Razor views, server-rendered HTML |
| Styling | Bootstrap plus custom CSS |
| Authentication | ASP.NET Core Identity |
| ORM | Entity Framework Core |
| Local database | SQLite |
| Catalog source | Seeded internal movie catalog |
| External API | Deferred optional enhancement |
| Tests | xUnit or equivalent .NET test framework |

## 3. Source Reconciliation

The supplied context has a few modeling variations. Stage 1 resolves them as follows:

- The SQL schema uses PostgreSQL syntax such as `BIGSERIAL`; implementation should use EF Core migrations targeting SQLite for local development.
- The class table includes `Movie.genre`; implementation should use `Genre` plus `MovieGenre` for many-to-many genre categorization.
- The parts document mentions direct user-to-watchlist-item relationships; implementation should route this through `Watchlist` and `WatchlistItem`.
- The source schema includes both `reviews.rating` and a separate `ratings.score`; implementation should treat `Rating.Score` as the authoritative numeric rating and keep reviews focused on written text.
- `WatchedMovie` and `ViewingHistoryItem` refer to the same concept. Use `ViewingHistoryItem`.

These are not blocking contradictions for Stage 1. They are documented decisions for later implementation.

## 4. User Types

### Anonymous Visitor

An anonymous visitor can:

- Browse public movie listings.
- Search and filter movies.
- View movie details.
- Register for an account.
- Log in.

Anonymous visitors cannot create watchlist entries, ratings, reviews, or watched-history records.

### Registered User

A registered user can:

- Log in and log out.
- Manage basic profile details.
- Add movies to a personal watchlist.
- Remove movies from their watchlist.
- Mark movies watched or unwatched.
- Rate movies from 1 to 5.
- Create, edit, and delete their own reviews.
- View watched history.

### Admin User

Admin catalog management is optional and deferred. It is only needed if a later stage requires internal catalog maintenance beyond seeded data.

## 5. MVP Functional Requirements

### FR-1 Account Registration

The app shall allow users to create accounts using username, email, and password through ASP.NET Core Identity.

### FR-2 Login And Logout

The app shall allow registered users to log in and log out. Authenticated users can access personal watchlist, rating, review, profile, and history features.

### FR-3 Browse Movies

The app shall display a browsable movie catalog. Movie cards should show poster, title, release year, genres, runtime where available, rating summary where available, and an add-to-watchlist action.

### FR-4 Search, Filter, And Sort Movies

The app shall support searching by title, filtering by genre/year/rating/watched status where relevant, and sorting by title, release year, popularity if available, or user rating if available.

### FR-5 View Movie Details

The movie details page shall show poster, title, release year, genres, runtime, description, watchlist action, watched action, rating form, review form, and other reviews.

### FR-6 Manage Watchlist

Authenticated users shall be able to add movies to a watchlist, remove them, view their saved movies, sort/filter the list, and open movie details.

Rules:

- A movie must not be duplicated in the same watchlist.
- Users may only manage their own watchlists.
- Removing a watchlist item must not delete the movie catalog record.

### FR-7 Mark Watched And Unwatched

Authenticated users shall be able to mark watchlist movies as watched or unwatched.

Rules:

- Marking watched updates the watchlist item status to `watched`.
- Marking watched creates or updates a `ViewingHistoryItem`.
- Marking unwatched reverts the watchlist item to a non-watched status, normally `planned`.
- The implementation may delete the related history record or mark it inactive; the MVP should prefer the simpler consistent behavior.

### FR-8 Watched History

Authenticated users shall be able to view watched movies, watched dates, rating status, review status, and review actions.

### FR-9 Ratings

Authenticated users shall be able to create, edit, and delete their own ratings.

Rules:

- Rating score must be from 1 to 5.
- A user can have at most one rating per movie.
- Users cannot edit another user's rating.

### FR-10 Reviews

Authenticated users shall be able to create, edit, and delete their own reviews.

Rules:

- A user can have at most one review per movie.
- Reviews store created and updated timestamps.
- Users cannot edit another user's review.
- The UI should clearly state whether reviews are public or private. MVP default: reviews shown on movie details are public to logged-in and anonymous viewers unless later changed.

### FR-11 Genre Categorization

The app shall support many-to-many movie genre categorization through `MovieGenre`. Genre names must be unique.

## 6. Non-Functional Requirements

- Performance: common pages should load quickly; search/watchlist pages should target under 2 seconds in normal local use.
- Reliability: failed actions should show clear messages and not lose user data.
- Security: use Identity, authorization checks, input validation, anti-forgery tokens, EF Core LINQ, and Razor encoding.
- Privacy: personal watchlists, ratings, reviews, and history must follow clear privacy rules.
- Usability: core workflows should require minimal steps.
- Accessibility: support keyboard navigation, readable labels, accessible names, and non-color-only status.
- Responsiveness: layouts must work on desktop, tablet, and mobile.
- Maintainability: keep authentication, movie browsing, watchlists, ratings, reviews, history, data access, and view models separated.
- Testability: support automated tests for core workflows.

## 7. Domain Model

### ApplicationUser

Represents a registered user.

Fields:

- `Id`
- `UserName`
- `Email`
- `CreatedAt`

Identity handles password and account fields.

Relationships:

- Has many `Watchlist`
- Has many `ViewingHistoryItem`
- Has many `Rating`
- Has many `Review`

### Movie

Represents a movie in the catalog.

Fields:

- `Id`
- `Title`
- `ReleaseYear`
- `Description`
- `PosterUrl`
- `Runtime`
- `ExternalApiId`

Relationships:

- Has many `WatchlistItem`
- Has many `ViewingHistoryItem`
- Has many `Rating`
- Has many `Review`
- Has many `MovieGenre`

### Genre

Represents a movie category.

Fields:

- `Id`
- `Name`

Constraint:

- `Name` must be unique.

### MovieGenre

Join entity between `Movie` and `Genre`.

Fields:

- `MovieId`
- `GenreId`

Primary key:

- Composite key: `MovieId`, `GenreId`

### Watchlist

Represents a user-owned list.

Fields:

- `Id`
- `UserId`
- `Name`
- `CreatedAt`

Constraint:

- Unique `UserId`, `Name`

### WatchlistItem

Represents a movie in a watchlist.

Fields:

- `Id`
- `WatchlistId`
- `MovieId`
- `Status`
- `AddedAt`

Allowed statuses:

- `planned`
- `watching`
- `watched`
- `dropped`

Constraint:

- Unique `WatchlistId`, `MovieId`

### ViewingHistoryItem

Represents a watched movie record.

Fields:

- `Id`
- `UserId`
- `MovieId`
- `WatchedAt`

### Rating

Represents a user's numeric score.

Fields:

- `Id`
- `UserId`
- `MovieId`
- `Score`

Constraints:

- `Score` must be from 1 to 5.
- Unique `UserId`, `MovieId`

### Review

Represents a user's written review.

Fields:

- `Id`
- `UserId`
- `MovieId`
- `Text`
- `CreatedAt`
- `UpdatedAt`

Constraint:

- Unique `UserId`, `MovieId`

## 8. Routes And Pages

### Home / Browse Movies

Routes:

- `GET /`
- `GET /Movies`

Purpose:

- Main discovery page with search, filters, sorting, popular/recent seeded movies, and movie cards.

### Movie Details

Route:

- `GET /Movies/Details/{id}`

Purpose:

- Movie details, add-to-watchlist action, watched action, user rating, user review, and other reviews.

### My Watchlist

Route:

- `GET /Watchlists`

Purpose:

- Authenticated user's saved movie list with sort/filter, mark watched, remove, and detail navigation.

### Watched / History

Route:

- `GET /History`

Purpose:

- Authenticated user's watched movie history with watched dates, ratings, and review actions.

### Login / Signup

Routes:

- ASP.NET Core Identity account routes, such as `/Identity/Account/Login` and `/Identity/Account/Register`.

## 9. Controllers

- `HomeController`: home and browse entry point.
- `MoviesController`: search, filter, sort, details.
- `WatchlistsController`: watchlist display, add, remove, mark watched, mark unwatched.
- `HistoryController`: watched history and optional watch-again workflow.
- `RatingsController`: create/update/delete ratings.
- `ReviewsController`: create/edit/delete reviews.
- `ProfileController`: profile/account landing page if needed beyond Identity.
- `AdminMoviesController`: optional later catalog management.

## 10. Services

- `MovieSearchService`: query, filter, sort movie catalog results.
- `WatchlistService`: add/remove movies, prevent duplicates, enforce ownership.
- `ViewingHistoryService`: mark watched/unwatched and retrieve history.
- `RatingService`: save/update/delete ratings and validate scores.
- `ReviewService`: create/edit/delete reviews and enforce ownership.
- `GenreService`: retrieve genres and support filtering.
- `ExternalMovieApiService`: optional later abstraction.

## 11. View Models

- `MovieCardViewModel`
- `MovieSearchViewModel`
- `MovieDetailsViewModel`
- `WatchlistViewModel`
- `WatchlistItemViewModel`
- `HistoryViewModel`
- `RatingFormViewModel`
- `ReviewFormViewModel`

## 12. Validation Rules

- Username is required.
- Email is required and must be valid.
- Password follows Identity rules.
- Review text has a maximum length.
- Rating score must be from 1 to 5.
- Movie title is required.
- Release year must be null or no earlier than 1888.
- Runtime must be null or greater than 0.
- Anonymous users attempting personal actions are redirected to login.
- Users can only manage their own watchlists, ratings, reviews, and history records.

## 13. Error Handling

The app should show clear messages when:

- A movie is already in the watchlist.
- Rating value is invalid.
- Review save fails.
- Movie is not found.
- A user attempts unauthorized access.
- A database operation fails.
- External API is unavailable, if implemented later.

## 14. Testing Plan

Automated tests should cover:

- Register/login flow
- Movie browse page loads
- Search returns expected movies
- Genre filter works
- Movie details displays required fields
- Add movie to watchlist
- Prevent duplicate watchlist entry
- Remove movie from watchlist
- Mark watched creates history record
- Mark unwatched updates status/history
- Save rating
- Reject invalid rating
- Edit/delete rating
- Create review
- Edit review
- Delete review
- Prevent users from editing another user's records

## 15. Deferred Scope

- External movie API integration
- Popular/trending/recent movies from an API
- Trailer links from provider metadata
- Cast/director metadata from provider metadata
- Admin content management
- Recommendations
- Poster caching and optimization
- Public/private review toggle
- Advanced pagination
- Backup/recovery automation

## 16. Assumptions

- The MVP is a local development app, not a deployed production service.
- The .NET SDK will be available before compilation, migrations, tests, or runtime verification.
- Seeded movies are sufficient for MVP browsing/search/filtering.
- Reviews are public by default on movie details unless a later privacy requirement changes this.
- Bootstrap is acceptable as the baseline responsive UI framework.
