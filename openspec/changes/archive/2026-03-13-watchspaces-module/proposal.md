## Why

BloomWatch's core value proposition is collaborative anime tracking, but without a WatchSpaces module there is no way for users to form groups, invite others, or share a space to track anime together. The Identity module is complete; WatchSpaces is the next foundational layer every other module (AnimeTracking, Analytics) depends on.

## What Changes

- Introduce a new `WatchSpaces` module following the same DDD modular structure as `Identity`
- Add the `WatchSpace` aggregate root with `WatchSpaceMember` and `Invitation` child entities
- Expose HTTP endpoints for creating spaces, inviting members by email, accepting invitations, and listing a user's spaces
- Add the `watch_spaces` PostgreSQL schema with migrations for `watch_spaces`, `watch_space_members`, and `invitations` tables
- Publish integration events from WatchSpaces so downstream modules (AnimeTracking) can react to membership changes

## Capabilities

### New Capabilities

- `watch-space-management`: Create, rename, and retrieve watch spaces; enforce ownership rules; transfer ownership
- `watch-space-invitations`: Invite a user by email to a space, accept or decline an invitation via a token, handle expiry
- `watch-space-membership`: List members of a space, remove a member, enforce the "at least one owner" invariant

### Modified Capabilities

<!-- No existing specs require changes. -->

## Impact

- **New module projects**: `BloomWatch.Modules.WatchSpaces.Domain`, `.Application`, `.Infrastructure`, `.Contracts`
- **New test projects**: `BloomWatch.Modules.WatchSpaces.UnitTests`, `BloomWatch.Modules.WatchSpaces.IntegrationTests`
- **API host**: endpoint registration for WatchSpaces routes; module DI wiring
- **Database**: new `watch_spaces` schema with three tables and EF Core migrations
- **Identity dependency**: WatchSpaces reads `identity.users` (via integration events or cross-module query) to resolve invited email addresses to user IDs
- **Downstream readiness**: once complete, AnimeTracking can reference `WatchSpaceId` and assume membership rules are enforced here
