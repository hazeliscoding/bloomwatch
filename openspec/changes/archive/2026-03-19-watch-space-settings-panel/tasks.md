## 1. Backend — Add displayName to MemberDetail

- [x] 1.1 Add `IUserDisplayNameLookup` abstraction to `WatchSpaces.Application/Abstractions/` (same pattern as the Analytics module: `Task<Dictionary<Guid, string>> GetDisplayNamesAsync(IEnumerable<Guid> userIds, CancellationToken ct)`)
- [x] 1.2 Implement `UserDisplayNameLookup` in `WatchSpaces.Infrastructure/CrossModule/` using the existing `IdentityReadDbContext` to batch-query display names
- [x] 1.3 Add `DisplayName` property to the `MemberDetail` record in `GetWatchSpaceByIdQuery.cs` (change from 3-param to 4-param: `UserId`, `DisplayName`, `Role`, `JoinedAt`)
- [x] 1.4 Update `GetWatchSpaceByIdQueryHandler` to inject `IUserDisplayNameLookup`, batch-resolve member display names, and populate `MemberDetail.DisplayName`
- [x] 1.5 Register `IUserDisplayNameLookup`/`UserDisplayNameLookup` in the WatchSpaces module's `ServiceCollectionExtensions`
- [x] 1.6 Update existing WatchSpaces integration tests that assert on the `GET /watchspaces/{id}` response to verify `displayName` is present in each member object

## 2. Frontend — TypeScript Models

- [x] 2.1 Add `WatchSpaceDetail` interface to `watch-space.model.ts` with `watchSpaceId`, `name`, `createdAt`, `members: MemberDetail[]`
- [x] 2.2 Add `MemberDetail` interface with `userId`, `displayName`, `role`, `joinedAt`

## 3. Frontend — WatchSpaceService Methods

- [x] 3.1 Add `getWatchSpaceById(id: string)` method returning `Observable<WatchSpaceDetail>` via `GET /watchspaces/{id}`
- [x] 3.2 Add `renameWatchSpace(id: string, name: string)` method via `PATCH /watchspaces/{id}`
- [x] 3.3 Add `removeMember(spaceId: string, userId: string)` method via `DELETE /watchspaces/{spaceId}/members/{userId}`
- [x] 3.4 Add `transferOwnership(spaceId: string, newOwnerId: string)` method via `POST /watchspaces/{spaceId}/transfer-ownership`
- [x] 3.5 Add `leaveWatchSpace(spaceId: string)` method via `DELETE /watchspaces/{spaceId}/members/me`

## 4. Frontend — WatchSpaceDetail Component

- [x] 4.1 Rewrite `watch-space-detail.ts` as a standalone component with signals: `detail` (WatchSpaceDetail | null), `isLoading`, `loadError`, `isOwner` (computed), `currentUserId` — load watch space on init using route param `:id`
- [x] 4.2 Create `watch-space-detail.html` template with: loading state, error state, space name heading (with inline edit for owner), and a members section
- [x] 4.3 Create `watch-space-detail.scss` with styles following the bloom design system (cards, spacing, badges)
- [x] 4.4 Implement inline rename: `isEditing` signal, `editName` signal, save/cancel methods that call `renameWatchSpace()` and update local `detail` signal on success
- [x] 4.5 Implement member list rendering: `@for` loop over `detail().members`, show `displayName`, role badge (pink/blue), join date via `DatePipe`
- [x] 4.6 Implement owner actions per member row: "Remove" button (non-owner members only) with `confirm()` prompt, calls `removeMember()`, removes from local member list on success
- [x] 4.7 Implement transfer ownership action per member row: "Transfer Ownership" button (non-owner members only, owner view only) with `confirm()` prompt, calls `transferOwnership()`, swaps roles in local member list on success
- [x] 4.8 Implement "Leave Space" action for non-owner members: button visible only to non-owners, `confirm()` prompt, calls `leaveWatchSpace()`, redirects to `/watch-spaces` on success

## 5. Current User Identity

- [x] 5.1 Ensure the current user's ID is accessible in the component (from JWT token or auth service) to compute `isOwner` and determine which member row is "self"

## 6. Documentation

- [x] 6.1 Update `docs/user-stories.md` to mark Story 8.2 as ✅ Done
