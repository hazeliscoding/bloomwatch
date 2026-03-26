# BloomWatch — User Stories (MVP Source of Truth)

**Document version:** 2.2
**Created:** 2026-03-13
**Last updated:** 2026-03-26
**Scope:** Phases 1–3 (full MVP) plus AniList discovery from Phase 4 (required by Phase 2)
**Tech stack:** .NET 10 / ASP.NET Core Minimal APIs, PostgreSQL, EF Core, Angular, AniList GraphQL

---

## Reading This Document

### Story status

| Badge | Meaning |
|---|---|
| ✅ Done | Story is fully implemented and tested |
| 📋 To Do | Story is not yet started |

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

| Story | Points | Status |
|---|---|---|
| 1.1 — User Registration (Backend) | 3 | ✅ Done |
| 1.2 — JWT Login (Backend) | 3 | ✅ Done |
| 1.3 — User Profile Endpoint (Backend) | 2 | ✅ Done |

<details>
<summary>Completed story details (1.1–1.3)</summary>

**Story 1.1 — User Registration (Backend):** `POST /auth/register` with BCrypt hashing, email uniqueness, validation. Module: Identity.

**Story 1.2 — JWT Login (Backend):** `POST /auth/login` returns signed HS256 JWT with 1-hour expiry. Module: Identity.

**Story 1.3 — User Profile Endpoint (Backend):** `GET /users/me` returns authenticated user's profile. Module: Identity.

</details>

---

## Epic 2 — Watch Spaces Management

**Goal:** Users can create shared spaces, invite others, manage membership, and control ownership.
**Module:** WatchSpaces
**Backend status:** Complete

| Story | Points | Status |
|---|---|---|
| 2.1 — Create a Watch Space | 3 | ✅ Done |
| 2.2 — List My Watch Spaces | 2 | ✅ Done |
| 2.3 — Get a Watch Space by ID | 2 | ✅ Done |
| 2.4 — Rename a Watch Space | 2 | ✅ Done |
| 2.5 — Transfer Watch Space Ownership | 3 | ✅ Done |
| 2.6 — Invite a Member by Email | 3 | ✅ Done |
| 2.7 — List Pending Invitations | 1 | ✅ Done |
| 2.8 — Revoke a Pending Invitation | 2 | ✅ Done |
| 2.9 — Accept an Invitation | 3 | ✅ Done |
| 2.10 — Decline an Invitation | 2 | ✅ Done |
| 2.11 — Remove a Member | 3 | ✅ Done |
| 2.12 — Leave a Watch Space | 2 | ✅ Done |

<details>
<summary>Completed story details (2.1–2.12)</summary>

**Story 2.1 — Create a Watch Space:** `POST /watchspaces` creates space, auto-assigns creator as Owner and Member.

**Story 2.2 — List My Watch Spaces:** `GET /watchspaces` returns spaces the current user is a member of with ID, name, role, and member count.

**Story 2.3 — Get a Watch Space by ID:** `GET /watchspaces/{id}` returns space details and member list. Members only (403 for non-members).

**Story 2.4 — Rename a Watch Space:** `PATCH /watchspaces/{id}` allows owner-only rename with validation.

**Story 2.5 — Transfer Watch Space Ownership:** Owner can transfer ownership to another member. Role swap ensures space always has exactly one owner.

**Story 2.6 — Invite a Member by Email:** `POST /watchspaces/{id}/invitations` generates unique token with expiry. Owner-only. 409 for duplicate invites.

**Story 2.7 — List Pending Invitations:** Returns pending invitations with email, dates, and expiry. Owner-only.

**Story 2.8 — Revoke a Pending Invitation:** Owner can cancel a pending invitation by ID. Already-used invitations return 409.

**Story 2.9 — Accept an Invitation:** `POST /watchspaces/invitations/{token}/accept` validates email match, expiry, and token state. Adds user as Member.

**Story 2.10 — Decline an Invitation:** Authenticated user can decline using invitation token. Email match validated.

**Story 2.11 — Remove a Member:** `DELETE /watchspaces/{id}/members/{userId}` is owner-only. Owner cannot remove themselves.

**Story 2.12 — Leave a Watch Space:** Self-removal for non-owner members. Owners must transfer ownership first.

</details>

---

## Epic 3 — AniList Discovery

**Goal:** The backend can search AniList for anime metadata and serve it to clients. Metadata is cached locally.
**Module:** AniListSync (discovery portion)
**Backend status:** Complete

| Story | Points | Status |
|---|---|---|
| 3.1 — AniList Search Proxy | 5 | ✅ Done |
| 3.2 — AniList Media Detail | 3 | ✅ Done |

<details>
<summary>Completed story details (3.1–3.2)</summary>

**Story 3.1 — AniList Search Proxy:** `GET /api/anilist/search?query=...` proxies to AniList GraphQL with in-memory caching. Returns media ID, titles, cover image, episodes, status, format, season, year, genres.

**Story 3.2 — AniList Media Detail:** `GET /api/anilist/media/{anilistMediaId}` returns full cached metadata. Cache miss triggers AniList fetch and stores in `anilist_sync.media_cache`.

</details>

---

## Epic 4 — Anime Tracking

**Goal:** Members of a watch space can add anime, track shared status and progress, record individual ratings, and log watch sessions.
**Module:** AnimeTracking
**Backend status:** Complete

| Story | Points | Status |
|---|---|---|
| 4.1 — Add Anime to a Watch Space | 5 | ✅ Done |
| 4.2 — List Anime in a Watch Space | 3 | ✅ Done |
| 4.3 — Get Anime Detail in a Watch Space | 3 | ✅ Done |
| 4.4 — Update Shared Anime Status and Metadata | 3 | ✅ Done |
| 4.5 — Update Individual Participant Progress | 3 | ✅ Done |
| 4.6 — Submit or Update a Personal Rating | 3 | ✅ Done |
| 4.7 — Record a Watch Session | 3 | ✅ Done |

<details>
<summary>Completed story details (4.1–4.7)</summary>

**Story 4.1 — Add Anime to a Watch Space:** `POST /watchspaces/{id}/anime` accepts AniList media ID with optional mood/vibe/pitch. Creates WatchSpaceAnime (Backlog, 0 episodes) and initial ParticipantEntry. Cross-module: verifies membership and fetches/caches AniList metadata.

**Story 4.2 — List Anime in a Watch Space:** `GET /watchspaces/{id}/anime` with optional `?status=` filter. Returns anime with shared state and participant summaries. Member-only.

**Story 4.3 — Get Anime Detail in a Watch Space:** `GET /watchspaces/{id}/anime/{watchSpaceAnimeId}` returns full aggregate: shared state, all participant entries with ratings, and watch session list.

**Story 4.4 — Update Shared Anime Status and Metadata:** `PATCH /watchspaces/{id}/anime/{watchSpaceAnimeId}` for partial update of sharedStatus, sharedEpisodesWatched, mood/vibe/pitch. Member-only.

**Story 4.5 — Update Individual Participant Progress:** `PATCH .../participant-progress` upserts the caller's ParticipantEntry with episodesWatched and individualStatus.

**Story 4.6 — Submit or Update a Personal Rating:** `PATCH .../participant-rating` accepts ratingScore (0.5–10, 0.5 increments) and optional ratingNotes. Upsert semantics.

**Story 4.7 — Record a Watch Session:** `POST .../sessions` accepts sessionDateUtc, startEpisode, endEpisode, notes. Episode range validation (start >= 1, end >= start).

</details>

---

## Epic 5 — Analytics and Dashboard

**Goal:** The system surfaces compatibility scores, rating gaps, shared statistics, and a random backlog picker so users can understand their shared taste at a glance.
**Module:** Analytics
**Backend status:** Complete

| Story | Points | Status |
|---|---|---|
| 5.1 — Watch Space Dashboard Summary Endpoint | 5 | ✅ Done |
| 5.2 — Compatibility Score Endpoint | 5 | ✅ Done |
| 5.3 — Rating Gaps Endpoint | 3 | ✅ Done |
| 5.4 — Shared Watch Stats Endpoint | 3 | ✅ Done |
| 5.5 — Random Backlog Picker Endpoint | 2 | ✅ Done |

<details>
<summary>Completed story details (5.1–5.5)</summary>

**Story 5.1 — Watch Space Dashboard Summary Endpoint:** `GET /watchspaces/{id}/dashboard` returns stats (totalShows, currentlyWatching, finished, episodesWatchedTogether), compatibility object, currentlyWatching list (up to 5), backlogHighlights (up to 5), and ratingGapHighlights (up to 3). Member-only.

**Story 5.2 — Compatibility Score Endpoint:** `GET /watchspaces/{id}/analytics/compatibility` computes score from anime rated by 2+ members. Formula: `max(0, round(100 - averageGap * 10))`. Returns score, averageGap, ratedTogetherCount, and label.

**Story 5.3 — Rating Gaps Endpoint:** `GET /watchspaces/{id}/analytics/rating-gaps` returns anime sorted by descending gap magnitude with each member's rating.

**Story 5.4 — Shared Watch Stats Endpoint:** `GET /watchspaces/{id}/analytics/shared-stats` returns totalEpisodesWatchedTogether, totalFinished, totalDropped, totalWatchSessions, mostRecentSessionDate.

**Story 5.5 — Random Backlog Picker Endpoint:** `GET /watchspaces/{id}/analytics/random-pick` selects one random anime from backlog. Returns 200 with null and message when backlog is empty.

</details>

---

## Epic 6 — Angular Frontend Shell

**Goal:** The Angular application has a working shell with routing, authentication guards, HTTP interceptors, and a responsive layout.
**Frontend status:** Complete

| Story | Points | Status |
|---|---|---|
| 6.1 — Angular Project Setup and Routing Shell | 3 | ✅ Done |
| 6.2 — HTTP Client and Auth Interceptor | 3 | ✅ Done |
| 6.3 — Theme System (Light and Dark Mode) | 2 | ✅ Done |

<details>
<summary>Completed story details (6.1–6.3)</summary>

**Story 6.1 — Angular Project Setup and Routing Shell:** Feature-based directory structure with lazy-loaded routes for all pages. Shell layout with nav bar for authenticated pages.

**Story 6.2 — HTTP Client and Auth Interceptor:** JWT injection interceptor, 401 redirect to `/login`, configurable API base URL. All feature services use shared ApiService.

**Story 6.3 — Theme System (Light and Dark Mode):** CSS custom property-based theme toggle. Persisted to localStorage. Accessible from navigation via bloom-theme-toggle component.

</details>

---

## Epic 7 — Auth Frontend

**Goal:** Users can register and log in through the Angular frontend.
**Frontend status:** Complete

| Story | Points | Status |
|---|---|---|
| 7.1 — Landing Page | 2 | ✅ Done |
| 7.2 — Registration Page | 3 | ✅ Done |
| 7.3 — Login Page | 2 | ✅ Done |
| 7.4 — Auth Route Guards | 2 | ✅ Done |

<details>
<summary>Completed story details (7.1–7.4)</summary>

**Story 7.1 — Landing Page:** Route `/`. Public marketing page with product description and CTAs for sign-in and create account.

**Story 7.2 — Registration Page:** Route `/register`. Form with display name, email, password, confirm password. Client-side validation. Auto-login on success.

**Story 7.3 — Login Page:** Route `/login`. Email and password form. JWT stored on success, redirect to `/watch-spaces`. Loading state during request.

**Story 7.4 — Auth Route Guards:** AuthGuard blocks unauthenticated access to protected routes. GuestGuard redirects authenticated users away from login/register.

</details>

---

## Epic 8 — Watch Spaces Frontend

**Goal:** Users can create, view, and manage watch spaces through the Angular UI, including the full invitation flow.
**Frontend status:** Complete

| Story | Points | Status |
|---|---|---|
| 8.1 — Watch Space Selector Page | 3 | ✅ Done |
| 8.2 — Watch Space Settings Panel | 3 | ✅ Done |
| 8.3 — Invitation Flow (Send and Manage) | 5 | ✅ Done |

<details>
<summary>Completed story details (8.1–8.3)</summary>

**Story 8.1 — Watch Space Selector Page:** Route `/watch-spaces`. Lists spaces with name, member count, role, and member preview avatars. Create modal with name validation. Kawaii-styled cards.

**Story 8.2 — Watch Space Settings Panel:** Route `/watch-spaces/:id/settings`. Inline rename (owner only), member list with role badges and join dates, remove member and transfer ownership actions (owner only), leave space (non-owner). Invite form and pending invitation management integrated.

**Story 8.3 — Invitation Flow (Send and Manage):** Invite form in settings (owner only) with email input. Pending invitation list with revoke. Invitee experience: `/watch-spaces/invitations/:token` shows space name with accept/decline buttons. Handles expired/invalid tokens gracefully.

</details>

---

## Epic 9 — Anime Tracking Frontend

**Goal:** Members can search for anime, add them to their watch space, view the shared list, update progress and status, rate anime, and log watch sessions.
**Frontend status:** Complete

| Story | Points | Status |
|---|---|---|
| 9.1 — Anime Search Modal | 5 | ✅ Done |
| 9.2 — Shared Anime List Page | 5 | ✅ Done |
| 9.3 — Anime Detail Page | 8 | ✅ Done |
| 9.4 — Inline Progress and Status Update Controls | 3 | ✅ Done |

<details>
<summary>Completed story details (9.1–9.4)</summary>

**Story 9.1 — Anime Search Modal:** Debounced search input calling AniList proxy. Results show cover image, title, episode count, status, format, genres. Already-added anime marked. Optional mood/vibe/pitch on add. Modal dismissible.

**Story 9.2 — Shared Anime List Page:** Route `/watch-spaces/:id/manage`. Tabs for status filtering (All, Backlog, Watching, Finished, Paused, Dropped). Anime cards with cover, title, shared status, episode progress, participant summaries. Inline status dropdown and episode increment (Story 9.4).

**Story 9.3 — Anime Detail Page:** Route `/watch-spaces/:id/anime/:animeId`. Full metadata display, shared status dropdown, episode stepper, mood/vibe/pitch display, per-participant progress and ratings, rating input (0.5–10 scale), watch session list, and "Log Session" form. Known gaps: mood/vibe/pitch edit controls are display-only; error state shows retry instead of redirecting.

**Story 9.4 — Inline Progress and Status Update Controls:** Episode count increment button and shared status dropdown on the anime list cards. Changes trigger PATCH endpoints with immediate UI update.

</details>

---

## Epic 10 — Analytics and Dashboard Frontend

**Goal:** The watch space dashboard gives users a visual, delightful overview of their shared tracking history, compatibility, and recommendations.
**Frontend status:** Complete

| Story | Points | Status |
|---|---|---|
| 10.1 — Watch Space Dashboard Page | 8 | ✅ Done |
| 10.2 — Compatibility Score Display Component | 3 | ✅ Done |
| 10.3 — Analytics Page | 5 | ✅ Done |
| 10.4 — Random Backlog Picker Component | 3 | ✅ Done |

<details>
<summary>Completed story details (10.1–10.4)</summary>

**Story 10.1 — Watch Space Dashboard Page:** Route `/watch-spaces/:id` (default view). Loads dashboard summary endpoint. Stat cards (Total Shows, Watching, Finished, Episodes Together), compatibility ring, backlog picker widget, currently watching grid with progress bars, backlog highlights, and rating gap highlights. Loading skeletons and error state with retry. Known gaps: backlog highlights missing mood/vibe/pitch tag display; no client-side limit guards (relies on API).

**Story 10.2 — Compatibility Score Display Component:** `bloom-compat-ring` shared component with SVG circular progress ring. Color is contextual (green 80+, yellow 50–79, pink below 50). Displays label and ratedTogetherCount context. Null state shows placeholder message. Reusable with `@Input()` binding. Used on dashboard and analytics page.

**Story 10.3 — Analytics Page:** Route `/watch-spaces/:id/analytics`. Loads compatibility, rating-gaps, and shared-stats endpoints in parallel via forkJoin. Two-column layout: compatibility section with `bloom-compat-ring` and average gap/rated-together breakdown, shared stats grid (episodes together, shows finished, shows dropped). Rating gaps section with visual bar comparison and score delta. Chart.js bar chart for rating comparison via ng2-charts. Per-section error handling with retry buttons. Loading skeletons. Responsive design with mobile breakpoint. Reduced motion support.

**Story 10.4 — Random Backlog Picker Component:** `bloom-backlog-picker` shared component. Accepts `spaceId` input, emits `picked` output. Fetches `GET .../analytics/random-pick`. Displays cover image, title, episode count, mood/vibe badges. "Reroll" button fetches new random result. "View Details" emits anime ID for navigation. Empty backlog shows friendly message. Loading skeleton state. Used on the dashboard page.

</details>

---

## Summary and Point Totals

### By epic

| Epic | Status | Stories | Total Points |
|---|---|---|---|
| Epic 1 — Authentication and Identity (Backend) | 3 ✅ Done | 3 | 8 |
| Epic 2 — Watch Spaces Management (Backend) | 12 ✅ Done | 12 | 28 |
| Epic 3 — AniList Discovery (Backend) | 2 ✅ Done | 2 | 8 |
| Epic 4 — Anime Tracking (Backend) | 7 ✅ Done | 7 | 23 |
| Epic 5 — Analytics and Dashboard (Backend) | 5 ✅ Done | 5 | 18 |
| Epic 6 — Angular Frontend Shell | 3 ✅ Done | 3 | 8 |
| Epic 7 — Auth Frontend | 4 ✅ Done | 4 | 9 |
| Epic 8 — Watch Spaces Frontend | 3 ✅ Done | 3 | 11 |
| Epic 9 — Anime Tracking Frontend | 4 ✅ Done | 4 | 21 |
| Epic 10 — Analytics and Dashboard Frontend | 4 ✅ Done | 4 | 19 |
| **Total** | **47 ✅ Done** | **47** | **153** |

### Completed vs remaining

| Category | Points |
|---|---|
| ✅ Done (all 47 stories across 10 epics) | 153 |
| 📋 To Do | 0 |
| Grand total (full MVP scope) | 153 |

**MVP is feature-complete.**

### Sprint history

All sprints are complete.

**Sprint 1 — Angular shell** *(Complete)*
- ~~Story 6.1 — Angular Project Setup (3 pts) — ✅ Done~~
- ~~Story 6.2 — HTTP Client and Auth Interceptor (3 pts) — ✅ Done~~
- ~~Story 6.3 — Theme System (2 pts) — ✅ Done~~
- Total: 8 pts

**Sprint 2 — Auth frontend + Watch Spaces frontend** *(Complete)*
- ~~Story 7.1 — Landing Page (2 pts) — ✅ Done~~
- ~~Story 7.2 — Registration Page (3 pts) — ✅ Done~~
- ~~Story 7.3 — Login Page (2 pts) — ✅ Done~~
- ~~Story 7.4 — Auth Route Guards (2 pts) — ✅ Done~~
- ~~Story 8.1 — Watch Space Selector Page (3 pts) — ✅ Done~~
- Total: 12 pts

**Sprint 3 — Watch Spaces frontend (management) + AniList backend** *(Complete)*
- ~~Story 8.2 — Watch Space Settings Panel (3 pts) — ✅ Done~~
- ~~Story 8.3 — Invitation Flow Frontend (5 pts) — ✅ Done~~
- ~~Story 3.1 — AniList Search Proxy (5 pts) — ✅ Done~~
- ~~Story 3.2 — AniList Media Detail (3 pts) — ✅ Done~~
- Total: 16 pts

**Sprint 4 — Anime Tracking backend (core)** *(Complete)*
- ~~Story 4.1 — Add Anime to a Watch Space (5 pts) — ✅ Done~~
- ~~Story 4.2 — List Anime in a Watch Space (3 pts) — ✅ Done~~
- ~~Story 4.3 — Get Anime Detail (3 pts) — ✅ Done~~
- ~~Story 4.4 — Update Shared Anime Status (3 pts) — ✅ Done~~
- Total: 14 pts

**Sprint 5 — Anime Tracking backend (progress, ratings, sessions)** *(Complete)*
- ~~Story 4.5 — Update Individual Participant Progress (3 pts) — ✅ Done~~
- ~~Story 4.6 — Submit or Update a Personal Rating (3 pts) — ✅ Done~~
- ~~Story 4.7 — Record a Watch Session (3 pts) — ✅ Done~~
- Total: 9 pts

**Sprint 6 — Anime Tracking frontend** *(Complete)*
- ~~Story 9.1 — Anime Search Modal (5 pts) — ✅ Done~~
- ~~Story 9.2 — Shared Anime List Page (5 pts) — ✅ Done~~
- ~~Story 9.4 — Inline Progress and Status Update Controls (3 pts) — ✅ Done~~
- Total: 13 pts

**Sprint 7 — Anime detail frontend + Analytics backend** *(Complete)*
- ~~Story 9.3 — Anime Detail Page (8 pts) — ✅ Done~~
- ~~Story 5.1 — Dashboard Summary Endpoint (5 pts) — ✅ Done~~
- ~~Story 5.5 — Random Backlog Picker Endpoint (2 pts) — ✅ Done~~
- Total: 15 pts

**Sprint 8 — Analytics backend (full) + Dashboard frontend** *(Complete)*
- ~~Story 5.2 — Compatibility Score Endpoint (5 pts) — ✅ Done~~
- ~~Story 5.3 — Rating Gaps Endpoint (3 pts) — ✅ Done~~
- ~~Story 5.4 — Shared Watch Stats Endpoint (3 pts) — ✅ Done~~
- ~~Story 10.1 — Watch Space Dashboard Page (8 pts) — ✅ Done~~
- ~~Story 10.2 — Compatibility Score Display Component (3 pts) — ✅ Done~~
- Total: 22 pts

**Sprint 9 — Analytics and Dashboard frontend (polish)** *(Complete)*
- ~~Story 10.3 — Analytics Page (5 pts) — ✅ Done~~
- ~~Story 10.4 — Random Backlog Picker Component (3 pts) — ✅ Done~~
- Total: 8 pts

---

## Known Gaps (minor, non-blocking)

These are implementation details noted during development that do not block MVP completion:

| Story | Gap |
|---|---|
| 9.3 | Mood/vibe/pitch edit controls are display-only (pencil icons present but no edit form wired) |
| 9.3 | Error state shows retry button instead of redirecting to anime list when anime is not found |
| 10.1 | Backlog highlights missing mood/vibe/pitch tag display (template only shows a "Backlog" badge) |
| 10.1 | No client-side limit guards for currently-watching (5) or backlog (5) sections — relies on API to cap results |
| 10.2 | Compatibility display was originally inline in the dashboard; now extracted as `bloom-compat-ring` shared component |

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
