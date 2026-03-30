## ADDED Requirements

### Requirement: Cross-module media cache reader for anime detail enrichment
The AnimeTracking module SHALL define an `IMediaCacheReader` adapter interface and use it in the `GetWatchSpaceAnimeDetailQueryHandler` to enrich the anime detail response with cached AniList metadata (genres, description, average score, popularity, tags, site URL, airing status).

#### Scenario: Successful enrichment from media cache
- **WHEN** the detail query handler returns a `WatchSpaceAnime` aggregate with `anilistMediaId = 154587` and the media cache contains an entry for that ID
- **THEN** the response SHALL include the cached `genres`, `description`, `averageScore`, `popularity`, `tags`, `siteUrl`, and `status` fields from the media cache entry

#### Scenario: Media cache entry not found
- **WHEN** the detail query handler returns a `WatchSpaceAnime` aggregate but no media cache entry exists for its `anilistMediaId`
- **THEN** the enrichment fields (`genres`, `description`, `averageScore`, `popularity`, `tags`, `siteUrl`, `airingStatus`) SHALL be `null` in the response and the request SHALL still return 200 OK with all other fields populated

#### Scenario: Adapter interface definition
- **WHEN** the `IMediaCacheReader` interface is defined in `AnimeTracking.Application.Abstractions`
- **THEN** it SHALL expose a method `GetMediaCacheAsync(int anilistMediaId, CancellationToken ct)` returning a DTO with nullable fields: `Genres` (string list), `Description` (string), `AverageScore` (int), `Popularity` (int), `Tags` (list of name/rank/spoiler), `SiteUrl` (string), `AiringStatus` (string)

### Requirement: Extended anime detail response DTO includes enriched metadata
The `GetWatchSpaceAnimeDetailResult` record SHALL be extended with optional fields for enriched AniList metadata.

#### Scenario: Response shape with enriched fields
- **WHEN** the anime detail endpoint returns 200 OK
- **THEN** the JSON response SHALL include the following additional nullable fields: `genres` (string array), `description` (string), `averageScore` (int), `popularity` (int), `tags` (array of objects with `name`, `rank`, `isMediaSpoiler`), `siteUrl` (string), `airingStatus` (string)

### Requirement: Frontend model includes enriched metadata fields
The `WatchSpaceAnimeDetail` TypeScript interface SHALL include fields for the enriched metadata.

#### Scenario: TypeScript interface fields
- **WHEN** the frontend `WatchSpaceAnimeDetail` interface is updated
- **THEN** it SHALL include: `tags?: AnimeTag[] | null` (where `AnimeTag` has `name: string`, `rank: number`, `isMediaSpoiler: boolean`), `siteUrl?: string | null`, `airingStatus?: string | null`

### Requirement: Anime detail page displays AniList tags
The anime detail page hero section SHALL display AniList tags as badges below the genres section.

#### Scenario: Tags rendered as badges
- **WHEN** the anime detail data includes a non-empty `tags` array
- **THEN** the component SHALL render each non-spoiler tag as a badge showing the tag name, ordered by rank descending

#### Scenario: Spoiler tags hidden by default
- **WHEN** a tag has `isMediaSpoiler: true`
- **THEN** the tag SHALL be visually obscured (blurred text with a "Spoiler" label) and SHALL NOT reveal its name until the user clicks to reveal

#### Scenario: Spoiler tag reveal interaction
- **WHEN** the user clicks on an obscured spoiler tag
- **THEN** the tag text SHALL become visible and the blur effect SHALL be removed

#### Scenario: No tags available
- **WHEN** the `tags` array is `null`, `undefined`, or empty
- **THEN** the tags section SHALL NOT be rendered

### Requirement: Anime detail page displays AniList external link
The anime detail page hero section SHALL display a clickable link to the anime's AniList page.

#### Scenario: External link rendered
- **WHEN** the anime detail data includes a non-null `siteUrl`
- **THEN** the component SHALL render an external link with text "View on AniList" that opens `siteUrl` in a new tab with `target="_blank"` and `rel="noopener noreferrer"`

#### Scenario: Site URL not available
- **WHEN** the `siteUrl` is `null` or `undefined`
- **THEN** the component SHALL construct a fallback URL as `https://anilist.co/anime/{anilistMediaId}` and render the link using that

### Requirement: Anime detail page displays airing status badge
The anime detail page meta line SHALL include the anime's airing status as a colored badge.

#### Scenario: Airing status rendered
- **WHEN** the anime detail data includes a non-null `airingStatus`
- **THEN** the component SHALL render a `bloom-badge` with human-readable text (e.g., "FINISHED" displays as "Finished", "RELEASING" displays as "Releasing") using status-appropriate colors: green for "Finished", lilac for "Releasing", blue for "Not Yet Released", yellow for "Cancelled"/"Hiatus"

#### Scenario: Airing status not available
- **WHEN** the `airingStatus` is `null` or `undefined`
- **THEN** no airing status badge SHALL be rendered

### Requirement: Anime detail page renders description as sanitized HTML
The anime detail page SHALL render the `description` field as HTML instead of plain text, with sanitization to prevent XSS.

#### Scenario: HTML description rendered
- **WHEN** the anime detail data includes a non-null `description` containing HTML tags (e.g., `<br>`, `<i>`, `<b>`)
- **THEN** the component SHALL render the description using Angular's `[innerHTML]` binding with sanitization that preserves safe HTML tags (`br`, `i`, `b`, `em`, `strong`) and strips unsafe content

#### Scenario: Description with no HTML
- **WHEN** the `description` is plain text with no HTML tags
- **THEN** the component SHALL render it as-is without modification

#### Scenario: Description is null
- **WHEN** the `description` is `null` or `undefined`
- **THEN** the description section SHALL NOT be rendered

### Requirement: Anime detail page limits displayed tags
The anime detail page SHALL display a maximum of 15 tags to prevent visual clutter, with non-spoiler tags shown first.

#### Scenario: More than 15 tags available
- **WHEN** the anime has more than 15 tags
- **THEN** the component SHALL display the top 15 tags sorted by rank descending, with non-spoiler tags prioritized over spoiler tags
