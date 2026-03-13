## Context

BloomWatch already has a working Identity module (user registration, authentication, JWT). WatchSpaces is the next module in the planned build order and is a hard prerequisite for AnimeTracking. The module must follow the same four-project DDD layout (`Domain`, `Application`, `Infrastructure`, `Contracts`) and the same conventions already established in the Identity module.

The PostgreSQL database uses one schema per module (`watch_spaces`). EF Core with Npgsql is the ORM. All HTTP routes use ASP.NET Core Minimal APIs. Cross-module communication uses strongly-typed integration events published to an in-process bus (matching the Identity pattern).

## Goals / Non-Goals

**Goals:**
- Implement the WatchSpace aggregate with full lifecycle (create, rename, transfer ownership, close)
- Implement token-based email invitations with configurable expiry (default 7 days)
- Enforce membership invariants (at least one owner at all times) inside the aggregate
- Expose REST endpoints under `/watchspaces` protected by JWT
- Produce EF Core migrations for the `watch_spaces` schema
- Publish `MemberJoinedWatchSpace` and `MemberLeftWatchSpace` integration events for AnimeTracking to consume later

**Non-Goals:**
- Real-time notifications (WebSockets, push) — deferred to a later phase
- Sending actual invitation emails — stub the email sender behind an interface; the infrastructure implementation is out of scope here
- AniList integration — no dependency on AniListSync module
- AnimeTracking features — this module does not own any anime data

## Decisions

### D1 — WatchSpace as the single aggregate root
**Decision:** `WatchSpaceMember` and `Invitation` live inside the `WatchSpace` aggregate; they are not separate aggregates.

**Rationale:** All membership invariants (e.g., "must have at least one owner") must be enforced atomically. Loading all members alongside the aggregate is acceptable given that watch spaces in the MVP have few members (2–10). Splitting into separate aggregates would require sagas or application-layer orchestration to enforce the owner invariant across transaction boundaries, adding complexity with no current benefit.

**Alternative considered:** Separate `Membership` aggregate — rejected because it forces the invariant check out of the domain layer.

---

### D2 — Invitation via opaque token, not magic link email
**Decision:** Invitations are stored as a UUID token in the database. The accepting user calls `POST /watchspaces/invitations/{token}/accept` with their JWT. The email send is stubbed as `IInvitationEmailSender`.

**Rationale:** This decouples the invitation workflow from an email provider. In a future phase a real SMTP/SES implementation can be plugged in without changing the domain or application layer. For now developers can retrieve the token directly from the database during testing.

**Alternative considered:** Sending a magic link that includes the JWT — rejected because it conflates authentication with invitation acceptance and complicates the flow.

---

### D3 — Role model: Owner / Member (two roles only for MVP)
**Decision:** `WatchSpaceRole` is an enum with two values: `Owner` and `Member`.

**Rationale:** The architecture doc calls out "roles may start simple: Owner, Member". Keeping to two roles avoids premature RBAC complexity. The domain model uses the enum in invariant checks, so adding roles later is a localised change.

---

### D4 — Cross-module user lookup via IUserReadModel (thin query interface)
**Decision:** To resolve whether an invited email belongs to a registered user, the WatchSpaces Application layer defines `IUserReadModel` with a single method `FindUserIdByEmailAsync`. The Infrastructure layer implements it by querying the `identity.users` table directly (read-only cross-schema query).

**Rationale:** This avoids introducing an HTTP call or a message bus round-trip for a synchronous lookup needed during invitation creation. Direct read-only cross-schema queries are explicitly allowed by the architecture doc ("Read models can denormalize across modules"). The interface keeps the Application layer free of infrastructure details.

**Alternative considered:** Publishing a request/response event to Identity — rejected as over-engineering for an in-process monolith.

---

### D5 — Integration events published after command handler commits
**Decision:** `MemberJoinedWatchSpace` and `MemberLeftWatchSpace` events are raised on the aggregate and dispatched by the Application layer after the EF Core `SaveChangesAsync` succeeds. Events are dispatched in-process (same pattern as Identity's `UserRegistered` event if present).

**Rationale:** Keeps the module boundary clean. Downstream modules subscribe to these events to maintain their own read models without querying `watch_spaces` tables directly.

## Risks / Trade-offs

- **Aggregate size as group grows** → Mitigation: Cap membership at a sensible limit (e.g., 20 members) enforced by a domain invariant; load is bounded.
- **Token collision on invitations** → Mitigation: Use `Guid.NewGuid()` — collision probability is negligible; add a unique DB constraint on the `token` column.
- **Stub email sender hides integration bugs** → Mitigation: Document the interface clearly; integration tests should assert that `IInvitationEmailSender.SendAsync` is called with correct arguments.
- **Direct cross-schema read on `identity.users`** → Trade-off: tight read coupling to Identity schema. Acceptable now; if Identity schema changes the Infrastructure implementation breaks at compile time via the EF model, making the coupling visible and fixable.

## Migration Plan

1. Create the `watch_spaces` schema migration (EF Core `add-migration`) targeting the `WatchSpacesDbContext`.
2. Register `WatchSpacesDbContext` in the API host alongside the existing `IdentityDbContext`.
3. Apply migrations in CI before running integration tests (`dotnet ef database update`).
4. Rollback: EF Core `database update <previous-migration>` drops the `watch_spaces` schema tables cleanly since no other module writes to them.

## Open Questions

- Should `Invitation` expiry be configurable via `appsettings.json` (default 7 days) or hard-coded in the domain? Recommendation: configurable, injected via `InvitationOptions` in Application layer.
- Should `WatchSpace.Name` have a maximum length enforced in the domain (e.g., 100 chars)? Recommendation: yes, validated in the value object or entity constructor and reflected in the DB column constraint.
