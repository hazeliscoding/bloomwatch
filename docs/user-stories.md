# BloomWatch — User Stories (MVP Source of Truth)

**Document version:** 1.0
**Created:** 2026-03-13
**Scope:** Phases 1–3 (full MVP) plus AniList discovery from Phase 4 (required by Phase 2)
**Tech stack:** .NET 10 / ASP.NET Core Minimal APIs, PostgreSQL, EF Core, Angular, AniList GraphQL

---

## Reading This Document

### Story status

| Badge | Meaning |
|---|---|
| Done | Story is fully implemented and tested |
| To Do | Story is not yet started |

### Story point scale (Fibonacci)

| Points | Reference |
|---|---|
| 1 | Trivial — single endpoint or component, no domain complexity |
| 2 | Simple — a few endpoints or a basic UI component with straightforward logic |
| 3 | Moderate — involves domain rules or a more complex UI interaction |
| 5 | Significant — multiple domain rules, cross-module concerns, or complex UI with state |
| 8 | Large — a full feature slice end-to-end or multiple interacting components |
| 13 | Very large — should be broken down further if possible |

### Story format

Each story follows: `As a [user], I want to [action], so that [benefit].`

---

## Table of Contents

1. [Epic 1 — Authentication and Identity](#epic-1--authentication-and-identity)
2. [Epic 2 — Watch Spaces Management](#epic-2--watch-spaces-management)
3. [Epic 3 — AniList Discovery](#epic-3--anilist-discovery)
4. [Epic 4 — Anime Tracking](#epic-4--anime-tracking)
5. [Epic 5 — Analytics and Dashboard](#epic-5--analytics-and-dashboard)
6. [Epic 6 — Angular Frontend Shell](#epic-6--angular-frontend-shell)
7. [Epic 7 — Auth Frontend](#epic-7--auth-frontend)
8. [Epic 8 — Watch Spaces Frontend](#epic-8--watch-spaces-frontend)
9. [Epic 9 — Anime Tracking Frontend](#epic-9--anime-tracking-frontend)
10. [Epic 10 — Analytics and Dashboard Frontend](#epic-10--analytics-and-dashboard-frontend)
11. [Summary and Point Totals](#summary-and-point-totals)

---

## Epic 1 — Authentication and Identity

**Goal:** Users can create a BloomWatch account and authenticate securely.
**Module:** Identity
**Backend status:** Complete

---

### Story 1.1 — User Registration (Backend)

**Status:** Done
**Points:** 3
**Sizing rationale:** Involves password hashing (BCrypt), email uniqueness validation, and structured domain/application/infrastructure layers — moderate complexity.

As a new user, I want to register for BloomWatch with my email, password, and display name, so that I can create an account and access the platform.

**Acceptance criteria:**
- `POST /api/auth/register` accepts `{ email, password, displayName }`
- Password is hashed with BCrypt before storage
- Duplicate email returns a 409 Conflict with a descriptive error message
- Invalid inputs (missing fields, malformed email, short password) return 400 Bad Request
- Successful registration returns 201 Created with the new user's ID and display name
- User is stored in `identity.users` with `is_active = true`

**Module:** Identity
**Endpoints:** `POST /api/auth/register`

---

### Story 1.2 — JWT Login (Backend)

**Status:** Done
**Points:** 3
**Sizing rationale:** Involves credential verification, BCrypt comparison, HS256 JWT generation with a 1-hour expiry, and proper error handling for bad credentials.

As a registered user, I want to log in with my email and password, so that I receive a JWT token I can use to authenticate subsequent requests.

**Acceptance criteria:**
- `POST /api/auth/login` accepts `{ email, password }`
- Returns a signed HS256 JWT with a 1-hour expiry on success
- JWT payload contains at minimum the user's ID and display name
- Invalid credentials return 401 Unauthorized
- Non-existent email returns the same 401 (no user enumeration)
- Token can be verified by protected endpoints using the same signing key

**Module:** Identity
**Endpoints:** `POST /api/auth/login`

---

### Story 1.3 — User Profile Endpoint (Backend)

**Status:** Done
**Points:** 2
**Sizing rationale:** Single authenticated GET endpoint returning the current user's profile data. No domain complexity beyond resolving the user ID from the JWT claim.

As an authenticated user, I want to fetch my own profile, so that the frontend can display my display name and account details without re-parsing the JWT.

**Acceptance criteria:**
- `GET /users/me` is protected (requires valid JWT)
- Returns `{ userId, email, displayName, accountStatus, isEmailVerified, createdAtUtc }`
- Returns 401 if the request is unauthenticated
- Returns 404 if the user no longer exists
- Response does not expose the password hash

**Module:** Identity
**Endpoints:** `GET /users/me`

---

## Epic 2 — Watch Spaces Management

**Goal:** Users can create shared spaces, invite others, manage membership, and control ownership.
**Module:** WatchSpaces
**Backend status:** Complete

---

### Story 2.1 — Create a Watch Space (Backend)

**Status:** Done
**Points:** 3
**Sizing rationale:** Involves creating the aggregate root, auto-assigning the creator as owner/member, and returning the new resource. Moderate domain setup.

As an authenticated user, I want to create a new watch space with a name, so that I have a shared space to track anime with friends.

**Acceptance criteria:**
- `POST /api/watch-spaces` accepts `{ name }`
- The requesting user is automatically assigned as the Owner of the new space
- The user is simultaneously added as a member
- Returns 201 Created with the new watch space ID and name
- Name is required and validated (non-empty, reasonable max length)
- The space is stored in `watch_spaces.watch_spaces`

**Module:** WatchSpaces
**Endpoints:** `POST /api/watch-spaces`

---

### Story 2.2 — List My Watch Spaces (Backend)

**Status:** Done
**Points:** 2
**Sizing rationale:** Simple read endpoint that filters watch spaces by the authenticated user's membership. No significant domain logic.

As an authenticated user, I want to see a list of all watch spaces I belong to, so that I can navigate between them.

**Acceptance criteria:**
- `GET /api/watch-spaces` returns only spaces the current user is a member of
- Response includes at minimum: watch space ID, name, the user's role, and member count
- An empty list returns 200 OK with an empty array (not 404)
- Spaces are ordered consistently (e.g., by creation date descending)

**Module:** WatchSpaces
**Endpoints:** `GET /api/watch-spaces`

---

### Story 2.3 — Get a Watch Space by ID (Backend)

**Status:** Done
**Points:** 2
**Sizing rationale:** Single GET with membership authorization check. Returns space details and member list.

As a watch space member, I want to view the details of a watch space, so that I can see its members and metadata.

**Acceptance criteria:**
- `GET /api/watch-spaces/{id}` returns space details for members only
- Response includes: ID, name, list of members (user ID, display name, role, joined date), pending invitation count
- Non-members receive 403 Forbidden
- Non-existent space returns 404 Not Found

**Module:** WatchSpaces
**Endpoints:** `GET /api/watch-spaces/{id}`

---

### Story 2.4 — Rename a Watch Space (Backend)

**Status:** Done
**Points:** 2
**Sizing rationale:** Simple mutation guarded by owner-only authorization. No multi-step domain logic.

As a watch space owner, I want to rename my watch space, so that I can keep its name meaningful as the group evolves.

**Acceptance criteria:**
- `PATCH /api/watch-spaces/{id}` (or dedicated rename endpoint) accepts `{ name }`
- Only the Owner of the space can rename it; Members receive 403
- Name validation applies (non-empty, max length)
- Returns 200 OK with the updated space details
- Returns 404 if the space does not exist

**Module:** WatchSpaces
**Endpoints:** `PATCH /api/watch-spaces/{id}`

---

### Story 2.5 — Transfer Watch Space Ownership (Backend)

**Status:** Done
**Points:** 3
**Sizing rationale:** Domain rule: the space must always have exactly one owner. Requires role swap logic and validation that the target is an existing member.

As a watch space owner, I want to transfer ownership to another member, so that someone else can manage the space if needed.

**Acceptance criteria:**
- Only the current Owner can initiate the transfer
- Target user must already be a member of the space
- After transfer, the previous owner becomes a Member and the target becomes the Owner
- The space is never left without an owner
- Returns 200 OK on success; 400 if the target is not a member; 403 if the caller is not the owner

**Module:** WatchSpaces

---

### Story 2.6 — Invite a Member by Email (Backend)

**Status:** Done
**Points:** 3
**Sizing rationale:** Generates a unique token, records the invitation with expiry, and applies owner-only and duplicate-invite guards.

As a watch space owner, I want to invite someone by their email address, so that they can join my watch space.

**Acceptance criteria:**
- `POST /api/watch-spaces/{id}/invitations` accepts `{ email }`
- Only the Owner can send invitations
- A unique invitation token is generated and stored with an expiry timestamp
- Inviting an email that is already a member returns a 409 Conflict
- Inviting the same pending email again returns 409 Conflict (or refreshes the token — consistent behavior documented)
- Returns 201 Created with the invitation ID (token is not returned in the response body)

**Module:** WatchSpaces
**Endpoints:** `POST /api/watch-spaces/{id}/invitations`

---

### Story 2.7 — List Pending Invitations (Backend)

**Status:** Done
**Points:** 1
**Sizing rationale:** Simple read endpoint, owner-only, filtered by pending status. Trivial domain logic.

As a watch space owner, I want to see all pending invitations for my space, so that I can track who has not yet responded.

**Acceptance criteria:**
- Returns only invitations with a pending status
- Each invitation shows: invitation ID, invited email, created date, expiry date
- Only the Owner can call this endpoint; Members get 403
- Returns 200 OK with empty array if no pending invitations exist

**Module:** WatchSpaces

---

### Story 2.8 — Revoke a Pending Invitation (Backend)

**Status:** Done
**Points:** 2
**Sizing rationale:** Mutation that updates invitation status. Owner-only guard. Simple validation.

As a watch space owner, I want to revoke a pending invitation, so that the invitee can no longer use that token to join.

**Acceptance criteria:**
- Owner can cancel a pending invitation by its ID
- Revoking an already-accepted or already-declined invitation returns 409 or 400
- Revoking a non-existent invitation returns 404
- Returns 200 OK on success

**Module:** WatchSpaces

---

### Story 2.9 — Accept an Invitation (Backend)

**Status:** Done
**Points:** 3
**Sizing rationale:** Token lookup with expiry validation, email match validation, and adding the user to the watch space as a Member. Multiple guard conditions.

As an invited user, I want to accept an invitation by its token, so that I become a member of the watch space.

**Acceptance criteria:**
- `POST /api/watch-spaces/invitations/{token}/accept` is an authenticated endpoint
- The authenticated user's email must match the invitation's invited email
- Expired tokens return 400 or 410 Gone
- Tokens already used (accepted/declined/revoked) return 409
- On success, the user is added as a Member and the invitation status is updated to accepted
- Returns 200 OK with the watch space ID and name

**Module:** WatchSpaces
**Endpoints:** `POST /api/watch-spaces/invitations/{token}/accept`

---

### Story 2.10 — Decline an Invitation (Backend)

**Status:** Done
**Points:** 2
**Sizing rationale:** Similar to accept but simpler — just marks the invitation declined, no membership record created.

As an invited user, I want to decline an invitation, so that the inviter knows I am not joining.

**Acceptance criteria:**
- Authenticated user can decline using the invitation token
- Email match is validated as with accept
- Invitation status is set to declined
- Returns 200 OK
- Already-used tokens return 409

**Module:** WatchSpaces

---

### Story 2.11 — Remove a Member (Backend)

**Status:** Done
**Points:** 3
**Sizing rationale:** Owner-only operation with guard that the owner cannot remove themselves (use leave/transfer for that). Soft-deletes the membership record.

As a watch space owner, I want to remove a member from my watch space, so that I can manage membership.

**Acceptance criteria:**
- `DELETE /api/watch-spaces/{id}/members/{userId}` is owner-only
- Owner cannot remove themselves (must transfer ownership first)
- Attempting to remove a non-member returns 404
- On success, the member no longer appears in the member list
- Returns 200 OK or 204 No Content

**Module:** WatchSpaces
**Endpoints:** `DELETE /api/watch-spaces/{id}/members/{userId}`

---

### Story 2.12 — Leave a Watch Space (Backend)

**Status:** Done
**Points:** 2
**Sizing rationale:** Simple self-removal with guard that owners cannot leave without transferring first. No complex domain transitions.

As a non-owner member, I want to leave a watch space, so that I am no longer associated with it.

**Acceptance criteria:**
- Authenticated user can remove themselves from a space they are a member of
- The current Owner cannot leave without first transferring ownership (returns 400 with clear message)
- Leaving a space the user is not in returns 404
- Returns 200 OK on success

**Module:** WatchSpaces

---

## Epic 3 — AniList Discovery

**Goal:** The backend can search AniList for anime metadata and serve it to clients. Metadata is cached locally.
**Module:** AniListSync (discovery portion)
**Backend status:** Partial (1 of 2 stories done)

---

### Story 3.1 — AniList Search Proxy (Backend)

**Status:** Done
**Points:** 5
**Sizing rationale:** Requires building a GraphQL client (HttpClient-based), writing the AniList search query, mapping the response to internal DTOs, and layering in a short-lived cache. Cross-cutting concern between AniListSync and the API layer.

As an authenticated user, I want to search for anime by name, so that I can find the right anime to add to my watch space.

**Acceptance criteria:**
- `GET /api/anilist/search?query=...` proxies to AniList GraphQL search
- Query parameter is required; returns 400 if missing or blank
- Results include per anime: `anilistMediaId`, `titleRomaji`, `titleEnglish`, `coverImageUrl`, `episodes`, `status`, `format`, `season`, `seasonYear`, `genres`
- Results are ordered by AniList's relevance ranking
- Responses are cached in memory (short TTL, e.g. 5 minutes) to avoid redundant AniList calls for identical queries
- AniList API errors surface as 502 Bad Gateway with a descriptive message; they do not crash the endpoint
- Requires authentication

**Module:** AniListSync
**Endpoints:** `GET /api/anilist/search?query=...`

---

### Story 3.2 — AniList Media Detail (Backend)

**Status:** Done
**Points:** 3
**Sizing rationale:** Single-record fetch by AniList media ID. Cache hit path is trivial; cache miss triggers AniList GraphQL call and writes to `anilist_sync.media_cache`.

As an authenticated user, I want to fetch full details for a specific AniList anime by its ID, so that I can review its metadata before or after adding it to a watch space.

**Acceptance criteria:**
- `GET /api/anilist/media/{anilistMediaId}` returns full cached metadata for the given ID
- If not cached, fetches from AniList GraphQL and stores the result in `anilist_sync.media_cache`
- Cached record includes: all fields from Story 3.1 plus `description`, `averageScore`, `popularity`, `titleNative`
- Returns 404 if AniList does not know the ID
- Returns 200 with cached data (and `cachedAt` timestamp) if the cache is fresh (e.g. within 24 hours)
- AniList call failures return 502; the cache entry is not corrupted

**Module:** AniListSync
**Endpoints:** `GET /api/anilist/media/{anilistMediaId}`

---

## Epic 4 — Anime Tracking

**Goal:** Members of a watch space can add anime, track shared status and progress, record individual ratings, and log watch sessions.
**Module:** AnimeTracking
**Backend status:** To Do

---

### Story 4.1 — Add Anime to a Watch Space (Backend)

**Status:** To Do
**Points:** 5
**Sizing rationale:** Cross-module: must confirm watch space membership (WatchSpaces), look up or fetch AniList metadata (AniListSync), enforce the no-duplicate-AniList-ID rule, create the WatchSpaceAnime aggregate with metadata snapshots, and create the caller's ParticipantEntry. Multiple interacting domain rules.

As a watch space member, I want to add an anime to my watch space by its AniList ID, so that the group can track it together.

**Acceptance criteria:**
- `POST /api/watch-spaces/{id}/anime` accepts `{ anilistMediaId, mood?, vibe?, pitch? }`
- Caller must be a member of the watch space (403 otherwise)
- If the AniList media ID is already in the watch space, returns 409 Conflict
- AniList metadata is fetched (or retrieved from cache) and snapshots are stored: `preferredTitle`, `episodeCountSnapshot`, `coverImageUrlSnapshot`, `format`, `season`, `seasonYear`
- A new `WatchSpaceAnime` record is created with `sharedStatus = Backlog` and `sharedEpisodesWatched = 0`
- A `ParticipantEntry` is created for the adding user with `individualStatus = Backlog` and `episodesWatched = 0`
- `addedByUserId` is set to the authenticated user's ID
- Returns 201 Created with the new `WatchSpaceAnimeId` and metadata snapshot

**Module:** AnimeTracking
**Endpoints:** `POST /api/watch-spaces/{id}/anime`

---

### Story 4.2 — List Anime in a Watch Space (Backend)

**Status:** To Do
**Points:** 3
**Sizing rationale:** Read endpoint with membership guard, returns a list of WatchSpaceAnime with their shared status and basic participant summaries. Filtering by status is a useful optional query parameter.

As a watch space member, I want to see all anime in my watch space, so that I can browse the backlog, currently watching, and finished lists.

**Acceptance criteria:**
- `GET /api/watch-spaces/{id}/anime` is member-only (403 for non-members)
- Supports optional `?status=` filter (backlog, watching, finished, paused, dropped)
- Returns per anime: `watchSpaceAnimeId`, `anilistMediaId`, `preferredTitle`, `coverImageUrlSnapshot`, `episodeCountSnapshot`, `sharedStatus`, `sharedEpisodesWatched`, `addedAtUtc`
- Also returns a summary of participant entries per anime: each participant's `individualStatus` and `episodesWatched`
- Returns 200 OK with empty array if no anime are tracked yet
- Results are ordered consistently (e.g. by `addedAtUtc` descending within each status group)

**Module:** AnimeTracking
**Endpoints:** `GET /api/watch-spaces/{id}/anime`

---

### Story 4.3 — Get Anime Detail in a Watch Space (Backend)

**Status:** To Do
**Points:** 3
**Sizing rationale:** Single-record read with membership check. Returns full aggregate: shared state, all participant entries with ratings, and watch session list.

As a watch space member, I want to view full details for one anime in my watch space, so that I can see everyone's progress, ratings, and session history.

**Acceptance criteria:**
- `GET /api/watch-spaces/{id}/anime/{watchSpaceAnimeId}` is member-only
- Response includes all `WatchSpaceAnime` fields: shared status, shared progress, mood/vibe/pitch, metadata snapshots
- Response includes all `ParticipantEntry` records: userId, displayName, individualStatus, episodesWatched, ratingScore, ratingNotes, lastUpdatedAtUtc
- Response includes all `WatchSession` records: sessionId, sessionDateUtc, startEpisode, endEpisode, notes, createdByUserId
- Returns 404 if the anime is not tracked in this watch space
- Returns 403 if the caller is not a member

**Module:** AnimeTracking
**Endpoints:** `GET /api/watch-spaces/{id}/anime/{watchSpaceAnimeId}`

---

### Story 4.4 — Update Shared Anime Status and Metadata (Backend)

**Status:** To Do
**Points:** 3
**Sizing rationale:** Mutation of shared state on the WatchSpaceAnime aggregate. Requires membership check and validation of the status enum. Also covers optional mood/vibe/pitch fields.

As a watch space member, I want to update the shared status of an anime (e.g. move it from backlog to watching), so that the group's shared state stays current.

**Acceptance criteria:**
- `PATCH /api/watch-spaces/{id}/anime/{watchSpaceAnimeId}` accepts partial update body: `{ sharedStatus?, sharedEpisodesWatched?, mood?, vibe?, pitch? }`
- Caller must be a member of the watch space
- `sharedStatus` must be one of: `Backlog`, `Watching`, `Finished`, `Paused`, `Dropped`
- `sharedEpisodesWatched` must be >= 0 and, when `episodeCountSnapshot` is known, <= `episodeCountSnapshot`
- Only provided fields are updated (partial patch semantics)
- Returns 200 OK with the updated record
- Returns 400 for invalid status values or episode count violations

**Module:** AnimeTracking
**Endpoints:** `PATCH /api/watch-spaces/{id}/anime/{watchSpaceAnimeId}`

---

### Story 4.5 — Update Individual Participant Progress and Status (Backend)

**Status:** To Do
**Points:** 3
**Sizing rationale:** Creates or updates the calling user's ParticipantEntry for the anime. Domain rules: episodes cannot exceed known count; status is an enum. Upsert semantics for the entry.

As a watch space member, I want to update my own progress and individual status for an anime, so that others can see how far I have gotten.

**Acceptance criteria:**
- `PATCH /api/watch-spaces/{id}/anime/{watchSpaceAnimeId}/participant-progress` accepts `{ episodesWatched, individualStatus }`
- Caller must be a member; the operation applies only to the caller's own ParticipantEntry
- Creates the ParticipantEntry if it does not already exist (upsert)
- `individualStatus` must be one of the valid enum values
- `episodesWatched` must be >= 0 and, when `episodeCountSnapshot` is known, <= `episodeCountSnapshot`
- Returns 200 OK with the updated participant entry
- `lastUpdatedAtUtc` is updated on every successful call

**Module:** AnimeTracking
**Endpoints:** `PATCH /api/watch-spaces/{id}/anime/{watchSpaceAnimeId}/participant-progress`

---

### Story 4.6 — Submit or Update a Personal Rating (Backend)

**Status:** To Do
**Points:** 3
**Sizing rationale:** Involves the 0.5–10 scale constraint, rounding/validation logic, and upsert of the rating fields on the caller's ParticipantEntry. The constraint and Scale value object add moderate domain complexity.

As a watch space member, I want to rate an anime with a score from 0.5 to 10 and an optional note, so that my taste is recorded and contributes to compatibility analytics.

**Acceptance criteria:**
- `PATCH /api/watch-spaces/{id}/anime/{watchSpaceAnimeId}/participant-rating` accepts `{ ratingScore, ratingNotes? }`
- Caller must be a member; applies to the caller's own ParticipantEntry only
- `ratingScore` must be between 0.5 and 10 inclusive, in 0.5 increments
- Scores outside this range return 400 Bad Request
- `ratingNotes` is optional free-text (max length validated)
- Calling again with a new score overwrites the previous rating (upsert)
- Returns 200 OK with the updated participant entry including the new rating

**Module:** AnimeTracking
**Endpoints:** `PATCH /api/watch-spaces/{id}/anime/{watchSpaceAnimeId}/participant-rating`

---

### Story 4.7 — Record a Watch Session (Backend)

**Status:** To Do
**Points:** 3
**Sizing rationale:** Creates a WatchSession entity with episode range validation (start > 0, end >= start). The session date is provided by the caller (allows backdating). Requires membership check.

As a watch space member, I want to log a watch session with the episode range and date, so that the group has a shared history of what was watched and when.

**Acceptance criteria:**
- `POST /api/watch-spaces/{id}/anime/{watchSpaceAnimeId}/sessions` accepts `{ sessionDateUtc, startEpisode, endEpisode, notes? }`
- Caller must be a member of the watch space
- `startEpisode` must be >= 1
- `endEpisode` must be >= `startEpisode`
- `sessionDateUtc` must be a valid ISO 8601 timestamp (past or present; future sessions are allowed for scheduling but not required)
- `notes` is optional free-text
- `createdByUserId` is set to the authenticated user's ID
- Returns 201 Created with the new session ID and details

**Module:** AnimeTracking
**Endpoints:** `POST /api/watch-spaces/{id}/anime/{watchSpaceAnimeId}/sessions`

---

## Epic 5 — Analytics and Dashboard

**Goal:** The system surfaces compatibility scores, rating gaps, shared statistics, and a random backlog picker so users can understand their shared taste at a glance.
**Module:** Analytics
**Backend status:** To Do

---

### Story 5.1 — Watch Space Dashboard Summary Endpoint (Backend)

**Status:** To Do
**Points:** 5
**Sizing rationale:** Aggregates across multiple data sources: total shows by status, episodes watched together, in-progress anime, compatibility score, rating gap highlights, and backlog highlights. Cross-module read of AnimeTracking data. Result is a denormalized read model.

As a watch space member, I want to load a single dashboard endpoint, so that the frontend can display a complete overview without multiple round trips.

**Acceptance criteria:**
- `GET /api/watch-spaces/{id}/dashboard` is member-only
- Response includes:
  - `stats.totalShows` — total anime count across all statuses
  - `stats.currentlyWatching` — count with `sharedStatus = Watching`
  - `stats.finished` — count with `sharedStatus = Finished`
  - `stats.episodesWatchedTogether` — sum of all `sharedEpisodesWatched` across all anime in the space
  - `compatibility` — full compatibility object (score, averageGap, ratedTogetherCount, label)
  - `currentlyWatching` — list of up to 5 anime currently being watched, with progress
  - `backlogHighlights` — up to 5 randomly selected backlog items
  - `ratingGapHighlights` — up to 3 anime with the largest per-user rating gap
- Compatibility label maps score ranges to descriptive text (e.g. 90+ = "Very synced, with a little spice")
- If fewer than 2 members have rated any shared anime, compatibility returns `null` with a `"Not enough data"` message
- Returns 403 for non-members

**Module:** Analytics
**Endpoints:** `GET /api/watch-spaces/{id}/dashboard`

---

### Story 5.2 — Compatibility Score Endpoint (Backend)

**Status:** To Do
**Points:** 5
**Sizing rationale:** Requires collecting all anime rated by at least two members, computing per-anime score gaps, averaging them, and applying the formula `max(0, round(100 - averageGap * 10))`. Graceful degradation when data is sparse. The formula is simple but the data aggregation across ParticipantEntry records is moderately involved.

As a watch space member, I want to see our compatibility score, so that I can understand how aligned our anime tastes are.

**Acceptance criteria:**
- `GET /api/watch-spaces/{id}/analytics/compatibility` is member-only
- Considers only anime where at least 2 members have submitted a rating
- Per-anime gap = absolute difference between each pair of members' rating scores
- `averageGap` = mean of all per-anime gaps across qualifying anime
- `score = max(0, round(100 - averageGap × 10))`
- Response includes: `score`, `averageGap`, `ratedTogetherCount` (number of qualifying anime), `label`
- If `ratedTogetherCount` is 0, score is `null` and a human-readable explanation is returned
- Score is between 0 and 100 inclusive
- Score label examples: 90–100 = "Very synced", 70–89 = "Pretty aligned", 50–69 = "Some differences", below 50 = "Wildly different tastes"

**Module:** Analytics
**Endpoints:** `GET /api/watch-spaces/{id}/analytics/compatibility`

---

### Story 5.3 — Rating Gaps Endpoint (Backend)

**Status:** To Do
**Points:** 3
**Sizing rationale:** Read-model computation over ParticipantEntry data. Identifies anime where members' ratings diverge the most. Moderate data aggregation.

As a watch space member, I want to see which anime we rated most differently, so that we can discuss our taste differences.

**Acceptance criteria:**
- `GET /api/watch-spaces/{id}/analytics/rating-gaps` is member-only
- Returns all anime where at least 2 members have submitted ratings, sorted by descending gap magnitude
- Per result includes: `watchSpaceAnimeId`, `preferredTitle`, `coverImageUrlSnapshot`, and each member's `displayName` and `ratingScore`
- If fewer than 2 members have rated any anime, returns an empty array with a `"Not enough data"` flag
- Ties in gap magnitude are broken by title alphabetically

**Module:** Analytics
**Endpoints:** `GET /api/watch-spaces/{id}/analytics/rating-gaps`

---

### Story 5.4 — Shared Watch Stats Endpoint (Backend)

**Status:** To Do
**Points:** 3
**Sizing rationale:** Aggregation over WatchSession and WatchSpaceAnime records. Computes counts and totals but is primarily a read-model over existing data.

As a watch space member, I want to see aggregate statistics about our watch history, so that I can appreciate how much we have watched together.

**Acceptance criteria:**
- `GET /api/watch-spaces/{id}/analytics/shared-stats` is member-only
- Response includes:
  - `totalEpisodesWatchedTogether` — sum of `sharedEpisodesWatched` across all anime
  - `totalFinished` — count of anime with `sharedStatus = Finished`
  - `totalDropped` — count of anime with `sharedStatus = Dropped`
  - `totalWatchSessions` — count of recorded watch sessions
  - `mostRecentSessionDate` — date of the most recent watch session (null if none)
- Returns 403 for non-members

**Module:** Analytics
**Endpoints:** `GET /api/watch-spaces/{id}/analytics/shared-stats`

---

### Story 5.5 — Random Backlog Picker Endpoint (Backend)

**Status:** To Do
**Points:** 2
**Sizing rationale:** Simple random selection from the backlog filtered set. No complex domain logic — just a filtered query with randomization.

As a watch space member, I want the system to pick a random anime from our backlog, so that we can break decision paralysis when choosing what to watch next.

**Acceptance criteria:**
- `GET /api/watch-spaces/{id}/analytics/random-pick` is member-only
- Selects one anime at random from those with `sharedStatus = Backlog`
- Response includes: `watchSpaceAnimeId`, `preferredTitle`, `coverImageUrlSnapshot`, `episodeCountSnapshot`, `mood`, `vibe`, `pitch`
- If the backlog is empty, returns 200 with `null` and a `"Backlog is empty"` message (not 404)
- Randomness should be server-side (not predictable by the client)
- Each call may return a different result (no sticky selection)

**Module:** Analytics
**Endpoints:** `GET /api/watch-spaces/{id}/analytics/random-pick`

---

## Epic 6 — Angular Frontend Shell

**Goal:** The Angular application has a working shell with routing, authentication guards, HTTP interceptors, and a responsive layout.
**Frontend status:** Partial (1 of 3 stories done)

---

### Story 6.1 — Angular Project Setup and Routing Shell

**Status:** Done
**Points:** 3
**Sizing rationale:** Initial Angular app setup with feature-based structure, lazy-loaded routes, and basic layout component. No domain logic, but a meaningful amount of configuration and structural work.

As a developer, I want a working Angular project with lazy-loaded feature routes and a shared layout, so that all future features have a stable, consistent home.

**Acceptance criteria:**
- Angular project uses a feature-based directory structure: `core/`, `shared/`, `features/`
- Lazy-loaded routes exist for: `/`, `/login`, `/register`, `/watch-spaces`, `/watch-spaces/:id`, `/watch-spaces/:id/anime/:animeId`, `/settings`
- A shell layout component wraps authenticated pages with a nav bar and content area
- Public routes (landing, login, register) use a minimal layout without the nav bar
- App compiles and runs without errors

**Tech notes:** Angular, Angular Router, feature modules or standalone components

---

### Story 6.2 — HTTP Client and Auth Interceptor

**Status:** Done
**Points:** 3
**Sizing rationale:** API service layer with base URL configuration, JWT injection interceptor, and 401 redirect handling. Cross-cutting; affects every API call.

As a developer, I want all HTTP calls to the backend to automatically include the JWT token and handle 401 responses by redirecting to login, so that API calls work securely without manual token management.

**Acceptance criteria:**
- An Angular HTTP interceptor reads the JWT from storage (e.g. localStorage or a signal-based auth store) and attaches it as a `Bearer` token in the `Authorization` header
- A 401 response from any API call triggers a redirect to `/login` and clears the stored token
- API base URL is configurable via Angular environment files
- All feature API services extend or use a shared base HTTP client setup
- No duplicate interceptor logic in feature services

**Tech notes:** Angular `HttpClient`, `HTTP_INTERCEPTORS` or functional interceptors (Angular 15+)

---

### Story 6.3 — Theme System (Light and Dark Mode)

**Status:** To Do
**Points:** 2
**Sizing rationale:** CSS custom property-based theme toggle. No backend dependency. UI-only with a small amount of state management for persistence.

As a user, I want to switch between a light pastel mode and a dark mode, so that I can use BloomWatch comfortably in different lighting conditions.

**Acceptance criteria:**
- A toggle is accessible from the navigation or settings
- Selecting a theme applies it immediately across the entire app
- The selected theme is persisted to localStorage so it survives page reloads
- Both themes are visually consistent: text is readable, interactive elements are clearly visible
- Theme tokens are defined as CSS custom properties so component styles reference them rather than hard-coding colors

---

## Epic 7 — Auth Frontend

**Goal:** Users can register and log in through the Angular frontend.
**Frontend status:** To Do

---

### Story 7.1 — Landing Page

**Status:** To Do
**Points:** 2
**Sizing rationale:** Static marketing/intro page. No backend calls, no complex logic. Light UI work.

As a visitor, I want to see a landing page that explains what BloomWatch does, so that I understand the product before registering.

**Acceptance criteria:**
- Route: `/`
- Page is publicly accessible without authentication
- Includes: product name, a brief description, and clear CTAs for "Sign In" and "Create Account"
- Responsive design works on mobile and desktop
- Unauthenticated visitors are not redirected away from this page

---

### Story 7.2 — Registration Page

**Status:** To Do
**Points:** 3
**Sizing rationale:** Form with client-side validation, API call to register endpoint, inline error handling, and redirect on success.

As a new user, I want to fill in a registration form with my display name, email, and password, so that I can create a BloomWatch account.

**Acceptance criteria:**
- Route: `/register`
- Form fields: display name, email, password, confirm password
- Client-side validation: all fields required, email format, password minimum length, passwords must match
- Submitting the form calls `POST /api/auth/register`
- On success, the user is automatically logged in (token stored) and redirected to `/watch-spaces`
- Server-side errors (e.g. email already taken) are displayed inline near the relevant field or as a form-level alert
- Already-authenticated users are redirected away from this page (auth guard)

---

### Story 7.3 — Login Page

**Status:** To Do
**Points:** 2
**Sizing rationale:** Simpler than registration — two fields, one API call, straightforward error handling.

As a returning user, I want to enter my email and password to log in, so that I can access my watch spaces.

**Acceptance criteria:**
- Route: `/login`
- Form fields: email, password
- Submitting calls `POST /api/auth/login`
- On success, JWT is stored and the user is redirected to `/watch-spaces`
- Invalid credentials display a clear error message without revealing whether the email exists
- Loading state is shown while the request is in flight
- Already-authenticated users are redirected away from this page (auth guard)

---

### Story 7.4 — Auth Route Guards

**Status:** To Do
**Points:** 2
**Sizing rationale:** Two guards (authenticated and unauthenticated) applied to route definitions. Standard Angular pattern, no domain logic.

As a developer, I want route guards that prevent unauthenticated users from accessing protected pages and redirect logged-in users away from public-only pages, so that the app enforces access control at the routing level.

**Acceptance criteria:**
- `AuthGuard` blocks unauthenticated access to: `/watch-spaces/**`, `/settings`; redirects to `/login`
- `GuestGuard` (or equivalent) redirects authenticated users away from `/login` and `/register` to `/watch-spaces`
- Guards check the presence and basic validity of the stored JWT (e.g. not expired by checking the `exp` claim)
- Token expiry during a session triggers redirect to `/login` on the next protected navigation

---

## Epic 8 — Watch Spaces Frontend

**Goal:** Users can create, view, and manage watch spaces through the Angular UI, including the full invitation flow.
**Frontend status:** To Do

---

### Story 8.1 — Watch Space Selector Page

**Status:** To Do
**Points:** 3
**Sizing rationale:** List view with API call, create dialog, and navigation to a specific space. A few UI states (empty, loading, populated).

As an authenticated user, I want to see all my watch spaces listed on a selector page and create a new one, so that I can navigate to the right space or start a new one.

**Acceptance criteria:**
- Route: `/watch-spaces`
- Fetches and displays all watch spaces the user belongs to
- Each space card shows: name, member count, the user's role, and a link to enter the space
- A "Create Watch Space" action opens a modal or inline form with a name field
- Form submits to `POST /api/watch-spaces`; on success, the new space appears in the list immediately
- Loading and empty states are handled gracefully
- Creating a space with a blank name shows a validation error before submitting

---

### Story 8.2 — Watch Space Settings Panel

**Status:** To Do
**Points:** 3
**Sizing rationale:** Settings within a watch space: rename, view members, and for owners — remove members or transfer ownership. Multiple API calls and conditional UI by role.

As a watch space owner, I want a settings panel where I can rename the space, see all members, remove members, and transfer ownership, so that I can manage the space without leaving the app.

**Acceptance criteria:**
- Accessible from within the watch space view (e.g. a settings icon or tab)
- Displays current name with an inline edit control (owner only)
- Lists all members with their role and join date
- Owner sees a "Remove" button next to each non-owner member
- Owner sees a "Transfer Ownership" option that prompts for confirmation
- Non-owner members see the member list but no management controls
- All mutations call the relevant backend endpoints and update the UI on success

---

### Story 8.3 — Invitation Flow (Send and Manage)

**Status:** To Do
**Points:** 5
**Sizing rationale:** The invitation flow involves: invite form (email input, API call), pending invitation list, revoke action, and the accept/decline pages for invitees. Multiple screens and states; the most complex part of the WatchSpaces frontend.

As a watch space owner, I want to invite someone by email and manage pending invitations, so that I can grow my watch space membership.

**Acceptance criteria:**
- Invite form (owner only) accepts an email address and submits to `POST /api/watch-spaces/{id}/invitations`
- Success shows a confirmation; error (e.g. already a member) shows inline feedback
- Pending invitations list shows each invitation's email, sent date, and expiry
- Owner can revoke any pending invitation with a confirmation step
- Invitee experience: navigating to an invitation accept link shows a confirmation page with the space name
- Accepting calls `POST /api/watch-spaces/invitations/{token}/accept` and redirects to the watch space
- Declining calls the decline endpoint and shows a "declined" confirmation page
- Expired or invalid tokens show a user-friendly error page (not a blank 400 error)

---

## Epic 9 — Anime Tracking Frontend

**Goal:** Members can search for anime, add them to their watch space, view the shared list, update progress and status, rate anime, and log watch sessions.
**Frontend status:** To Do

---

### Story 9.1 — Anime Search Modal

**Status:** To Do
**Points:** 5
**Sizing rationale:** Debounced search input calling the AniList search proxy, result list with cover images and key metadata, and an "Add to Watch Space" action. Combines network state management, debouncing, and result rendering.

As a watch space member, I want to search for anime by name and add a result to my watch space, so that I can build the shared list without leaving the app.

**Acceptance criteria:**
- Search is triggered by a text input with debounce (e.g. 400ms) to avoid hammering the backend
- Results display: cover image, title (romaji and English if available), episode count, status, format, genres
- Anime already in the watch space are visually marked and cannot be added again
- Selecting a result shows an optional "Add details" step: mood, vibe, pitch free-text fields
- Submitting calls `POST /api/watch-spaces/{id}/anime`
- On success, the modal closes and the anime appears in the watch space list
- Loading, empty, and error states are handled gracefully
- The modal can be dismissed without adding anything

---

### Story 9.2 — Shared Anime List Page

**Status:** To Do
**Points:** 5
**Sizing rationale:** Tabbed or filtered list view grouped by status (backlog, watching, finished, paused, dropped), with cover images and participant progress indicators. Multiple UI states and filtering.

As a watch space member, I want to browse our anime list filtered by status, so that I can see what we are currently watching, what is in the backlog, and what we have finished.

**Acceptance criteria:**
- Route: `/watch-spaces/:id` (or a child route)
- Tabs or filter controls for: All, Backlog, Watching, Finished, Paused, Dropped
- Each anime card shows: cover image, title, shared status, shared episode progress (e.g. "Ep 5 / 24"), each member's individual episode count
- Clicking an anime card navigates to the anime detail page
- An "Add Anime" button opens the search modal (Story 9.1)
- Empty state for each status tab is handled with a friendly message
- Page fetches from `GET /api/watch-spaces/{id}/anime` with the appropriate `?status=` filter

---

### Story 9.3 — Anime Detail Page

**Status:** To Do
**Points:** 8
**Sizing rationale:** The richest screen in the app. Shows full anime metadata, per-participant progress and ratings, shared status controls, rating input, watch session list, and a "Log Session" form. Multiple API calls and a complex UI with multiple interactive sections.

As a watch space member, I want to see all details for one anime, so that I can update my progress, submit my rating, and log a watch session.

**Acceptance criteria:**
- Route: `/watch-spaces/:id/anime/:watchSpaceAnimeId`
- Displays AniList metadata snapshot: cover image, title, episode count, format, season/year, description (from full media detail endpoint), genres
- Shows shared status with a dropdown/selector that members can update
- Shows shared episode progress with a stepper or input (updates `sharedEpisodesWatched`)
- Mood/vibe/pitch display and edit controls
- Per-participant section: each member's individual status, episodes watched, rating score with stars or a numeric display, and rating notes
- Current user can update only their own participant progress and rating
- Rating input enforces 0.5–10 in 0.5 increments (e.g. a slider or segmented input)
- Watch session list shows all past sessions with dates, episode ranges, and notes
- "Log Watch Session" button opens an inline form or modal: date, start episode, end episode, notes
- All mutations provide immediate optimistic or reload-based feedback
- Returns to the anime list if the anime is not found

---

### Story 9.4 — Inline Progress and Status Update Controls

**Status:** To Do
**Points:** 3
**Sizing rationale:** Reusable components for updating participant progress and shared status, usable from both the list view and the detail page. Requires local state management and debounced or on-blur API calls.

As a watch space member, I want to quickly update my episode count or the shared status without navigating away from the anime list, so that updates are fast and low-friction.

**Acceptance criteria:**
- Episode count input on the list card or a quick-action button allows incrementing by 1 or entering a number directly
- Change triggers `PATCH .../participant-progress` after a short debounce or on blur
- Shared status can be updated from a dropdown on the list card (triggers `PATCH .../anime/{id}`)
- Success is reflected immediately in the UI without a full page reload
- Validation errors (e.g. exceeding episode count) surface inline without disrupting the whole list

---

## Epic 10 — Analytics and Dashboard Frontend

**Goal:** The watch space dashboard gives users a visual, delightful overview of their shared tracking history, compatibility, and recommendations.
**Frontend status:** To Do

---

### Story 10.1 — Watch Space Dashboard Page

**Status:** To Do
**Points:** 8
**Sizing rationale:** The primary "home" of a watch space. Loads the dashboard summary endpoint and renders multiple sections: snapshot cards, currently watching list, compatibility score, backlog highlights, rating gap highlights, and recent sessions. Significant UI composition with multiple subsections.

As a watch space member, I want to see a dashboard with a summary of our watch space activity, so that I can understand our progress at a glance.

**Acceptance criteria:**
- Route: `/watch-spaces/:id` (dashboard as the default view)
- Fetches `GET /api/watch-spaces/{id}/dashboard` on load
- Snapshot card row: Total Shows, Currently Watching, Finished, Episodes Watched Together
- Compatibility score section: large numeric score, label text, and `ratedTogetherCount` context; shows "Not enough ratings yet" gracefully when null
- Currently Watching section: up to 5 anime cards with cover image, title, and a progress bar showing `sharedEpisodesWatched / episodeCountSnapshot`
- Backlog Highlights section: up to 5 anime cards from the backlog with title, cover image, and mood/vibe/pitch tags if set
- Rating Gap Highlights: up to 3 anime with the largest divergence, showing each member's score side by side
- Loading skeleton states while data is fetching
- Error state if the dashboard endpoint fails

---

### Story 10.2 — Compatibility Score Display Component

**Status:** To Do
**Points:** 3
**Sizing rationale:** A standalone, visually distinct component that renders the compatibility score with a visual meter or ring, label, and contextual detail. Reusable on the dashboard and analytics page.

As a watch space member, I want to see our compatibility score displayed in a visually engaging way, so that the number feels meaningful and not just a raw figure.

**Acceptance criteria:**
- Component renders a circular progress ring or horizontal meter showing the score from 0–100
- Color is contextual: green/high for 80+, yellow/medium for 50–79, red/low for below 50
- The label (e.g. "Very synced, with a little spice") is displayed beneath the score
- `ratedTogetherCount` is shown as supporting context (e.g. "Based on 9 shared ratings")
- When score is null, the component shows a soft placeholder message ("Rate more anime together to unlock your compatibility score")
- Component accepts the full compatibility object as an input and is reusable

---

### Story 10.3 — Analytics Page

**Status:** To Do
**Points:** 5
**Sizing rationale:** Dedicated analytics page that calls the rating-gaps, shared-stats, and compatibility endpoints and renders them with chart components. More detailed than the dashboard summary.

As a watch space member, I want a dedicated analytics page with charts and tables showing our full compatibility breakdown and shared history, so that I can explore our taste alignment in more detail.

**Acceptance criteria:**
- Route: `/watch-spaces/:id/analytics` (or sub-tab of the watch space)
- Loads `GET .../analytics/compatibility`, `GET .../analytics/rating-gaps`, and `GET .../analytics/shared-stats` in parallel
- Compatibility section mirrors Story 10.2 component with additional breakdown
- Rating gaps section: table or card list of anime sorted by descending gap, showing each member's score
- Shared stats section: total episodes, total sessions, finish count, most recent session date
- Charts (bar, scatter, or simple visual comparison) for rating gaps — using a charting library (e.g. Chart.js via `ng2-charts` or `ngx-charts`)
- Graceful empty/insufficient-data states for each section
- Page is responsive on mobile

---

### Story 10.4 — Random Backlog Picker Component

**Status:** To Do
**Points:** 3
**Sizing rationale:** Interactive widget that fetches a random pick, displays the result with anime details, and allows the user to "reroll". Straightforward but requires a polished interactive feel.

As a watch space member, I want to hit a "Pick for me" button that randomly suggests an anime from our backlog, so that we can stop debating and just start watching something.

**Acceptance criteria:**
- Component appears on the dashboard (and optionally the anime list page)
- "Pick for me" button calls `GET /api/watch-spaces/{id}/analytics/random-pick`
- Result displays: cover image, title, episode count, mood/vibe/pitch if set
- A "Reroll" button fetches a new random result
- If the backlog is empty, the component shows "Your backlog is empty — add some anime first!"
- Loading state is shown while the request is in flight
- Clicking the result card navigates to the anime detail page

---

## Summary and Point Totals

### By epic

| Epic | Status | Stories | Total Points |
|---|---|---|---|
| Epic 1 — Authentication and Identity (Backend) | 3 Done | 3 | 8 |
| Epic 2 — Watch Spaces Management (Backend) | 12 Done | 12 | 28 |
| Epic 3 — AniList Discovery (Backend) | 1 Done / 1 To Do | 2 | 8 |
| Epic 4 — Anime Tracking (Backend) | To Do | 7 | 23 |
| Epic 5 — Analytics and Dashboard (Backend) | To Do | 5 | 18 |
| Epic 6 — Angular Frontend Shell | 1 Done / 2 To Do | 3 | 8 |
| Epic 7 — Auth Frontend | To Do | 4 | 9 |
| Epic 8 — Watch Spaces Frontend | To Do | 3 | 11 |
| Epic 9 — Anime Tracking Frontend | To Do | 4 | 21 |
| Epic 10 — Analytics and Dashboard Frontend | To Do | 4 | 19 |
| **Total** | | **47** | **153** |

### Completed vs remaining

| Category | Points |
|---|---|
| Done (Identity backend + WatchSpaces backend + AniList search + Angular shell) | 46 |
| To Do (remaining MVP) | 107 |
| Grand total (full MVP scope) | 153 |

### Suggested sprint groupings

These groupings are not prescriptive. They suggest a natural sequencing to unblock dependent work quickly.

**Sprint 1 — Angular shell**
- Story 6.1 — Angular Project Setup (3 pts)
- Story 6.2 — HTTP Client and Auth Interceptor (3 pts)
- Story 6.3 — Theme System (2 pts)
- Total: 8 pts

**Sprint 2 — Auth frontend + Watch Spaces frontend**
- Story 7.1 — Landing Page (2 pts)
- Story 7.2 — Registration Page (3 pts)
- Story 7.3 — Login Page (2 pts)
- Story 7.4 — Auth Route Guards (2 pts)
- Story 8.1 — Watch Space Selector Page (3 pts)
- Total: 12 pts

**Sprint 3 — Watch Spaces frontend (management) + AniList backend**
- Story 8.2 — Watch Space Settings Panel (3 pts)
- Story 8.3 — Invitation Flow Frontend (5 pts)
- Story 3.1 — AniList Search Proxy (5 pts)
- Story 3.2 — AniList Media Detail (3 pts)
- Total: 16 pts

**Sprint 4 — Anime Tracking backend (core)**
- Story 4.1 — Add Anime to a Watch Space (5 pts)
- Story 4.2 — List Anime in a Watch Space (3 pts)
- Story 4.3 — Get Anime Detail (3 pts)
- Story 4.4 — Update Shared Anime Status (3 pts)
- Total: 14 pts

**Sprint 5 — Anime Tracking backend (progress, ratings, sessions)**
- Story 4.5 — Update Individual Participant Progress (3 pts)
- Story 4.6 — Submit or Update a Personal Rating (3 pts)
- Story 4.7 — Record a Watch Session (3 pts)
- Total: 9 pts

**Sprint 6 — Anime Tracking frontend**
- Story 9.1 — Anime Search Modal (5 pts)
- Story 9.2 — Shared Anime List Page (5 pts)
- Story 9.4 — Inline Progress and Status Update Controls (3 pts)
- Total: 13 pts

**Sprint 7 — Anime detail frontend + Analytics backend**
- Story 9.3 — Anime Detail Page (8 pts)
- Story 5.1 — Dashboard Summary Endpoint (5 pts)
- Story 5.5 — Random Backlog Picker Endpoint (2 pts)
- Total: 15 pts

**Sprint 8 — Analytics backend (full) + Dashboard frontend**
- Story 5.2 — Compatibility Score Endpoint (5 pts)
- Story 5.3 — Rating Gaps Endpoint (3 pts)
- Story 5.4 — Shared Watch Stats Endpoint (3 pts)
- Story 10.1 — Watch Space Dashboard Page (8 pts)
- Total: 19 pts

**Sprint 9 — Analytics and Dashboard frontend (polish)**
- Story 10.2 — Compatibility Score Display Component (3 pts)
- Story 10.3 — Analytics Page (5 pts)
- Story 10.4 — Random Backlog Picker Component (3 pts)
- Total: 11 pts

---

## Business Rule Reference

This section cross-references all key business rules to the stories that implement them. Use this to verify no rule is left unimplemented.

| Business Rule | Implementing Story |
|---|---|
| Only members can see a watch space's anime | 4.2, 4.3 |
| Only owners can manage invitations | 2.6, 2.7, 2.8, 8.3 |
| Only owners can remove members | 2.11 |
| A watch space cannot contain duplicate AniList media IDs | 4.1 |
| Participant entry is unique per user per WatchSpaceAnime | 4.5, 4.6 |
| Ratings constrained to 0.5–10 scale | 4.6, 9.3 |
| Progress cannot exceed known episode count | 4.4, 4.5, 9.3, 9.4 |
| A space must always have an owner (no ownerless state) | 2.5, 2.12 |
| Owner cannot remove themselves (must transfer first) | 2.11, 2.12 |
| Compatibility considers only anime rated by 2+ members | 5.1, 5.2 |
| Analytics degrade gracefully when data is sparse | 5.1, 5.2, 5.3, 10.1, 10.2 |
| AniList sync failures must not block core workflows | 3.1, 3.2 |
| Invitation email must match the accepting user's email | 2.9 |
| Watch session: startEpisode >= 1, endEpisode >= startEpisode | 4.7 |
| Compatibility formula: max(0, round(100 - averageGap × 10)) | 5.2 |
