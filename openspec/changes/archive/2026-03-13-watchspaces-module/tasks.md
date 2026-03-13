## 1. Module Scaffolding

- [x] 1.1 Create `src/Modules/WatchSpaces/` folder with four projects: `BloomWatch.Modules.WatchSpaces.Domain`, `.Application`, `.Infrastructure`, `.Contracts`
- [x] 1.2 Add each project to `BloomWatch.slnx`
- [x] 1.3 Set project references: Domain ← Application ← Infrastructure; Contracts standalone; Api → Application + Contracts
- [x] 1.4 Add NuGet references: EF Core + Npgsql to Infrastructure; MediatR to Application (matching Identity conventions)

## 2. Domain Layer

- [x] 2.1 Define `WatchSpaceId`, `UserId` strongly-typed ID value objects (or alias existing ones from SharedKernel)
- [x] 2.2 Create `WatchSpaceRole` enum (`Owner`, `Member`)
- [x] 2.3 Create `WatchSpaceMember` entity with fields: `Id`, `UserId`, `Role`, `JoinedAtUtc`
- [x] 2.4 Create `InvitationStatus` enum (`Pending`, `Accepted`, `Declined`, `Revoked`)
- [x] 2.5 Create `Invitation` entity with fields: `Id`, `WatchSpaceId`, `InvitedByUserId`, `InvitedEmail`, `Token`, `Status`, `ExpiresAtUtc`, `CreatedAtUtc`, `AcceptedAtUtc`
- [x] 2.6 Create `WatchSpace` aggregate root with fields from the architecture doc; raise domain events on state changes
- [x] 2.7 Implement `WatchSpace.Create(name, creatorUserId)` — adds creator as `Owner`
- [x] 2.8 Implement `WatchSpace.Rename(newName, requestingUserId)` — validates caller is `Owner`
- [x] 2.9 Implement `WatchSpace.InviteMember(invitedEmail, invitedByUserId, expiresAt)` — validates no duplicate pending invitation, caller is `Owner`
- [x] 2.10 Implement `WatchSpace.AcceptInvitation(token, acceptingUserId, acceptingEmail, now)` — validates token, expiry, email match; adds member; marks invitation `Accepted`
- [x] 2.11 Implement `WatchSpace.DeclineInvitation(token, decliningEmail, now)` — validates token and email; marks `Declined`
- [x] 2.12 Implement `WatchSpace.RevokeInvitation(invitationId, requestingUserId)` — validates caller is `Owner` and invitation is `Pending`
- [x] 2.13 Implement `WatchSpace.RemoveMember(targetUserId, requestingUserId)` — enforces at-least-one-owner invariant
- [x] 2.14 Implement `WatchSpace.Leave(leavingUserId)` — enforces at-least-one-owner invariant
- [x] 2.15 Implement `WatchSpace.TransferOwnership(newOwnerId, requestingUserId)` — demotes requester to `Member`, promotes target to `Owner`
- [x] 2.16 Define `IWatchSpaceRepository` interface in Domain
- [x] 2.17 Write unit tests for all aggregate behaviors (invariant enforcement, happy paths, error paths)

## 3. Application Layer

- [x] 3.1 Define `IUserReadModel` interface with `FindUserIdByEmailAsync(email)` method
- [x] 3.2 Define `IInvitationEmailSender` interface with `SendAsync(invitedEmail, token, watchSpaceName)`
- [x] 3.3 Implement `CreateWatchSpaceCommand` + handler
- [x] 3.4 Implement `RenameWatchSpaceCommand` + handler
- [x] 3.5 Implement `InviteMemberCommand` + handler (uses `IUserReadModel` to verify email, `IInvitationEmailSender` to notify)
- [x] 3.6 Implement `AcceptInvitationCommand` + handler
- [x] 3.7 Implement `DeclineInvitationCommand` + handler
- [x] 3.8 Implement `RevokeInvitationCommand` + handler
- [x] 3.9 Implement `RemoveMemberCommand` + handler
- [x] 3.10 Implement `LeaveWatchSpaceCommand` + handler
- [x] 3.11 Implement `TransferOwnershipCommand` + handler
- [x] 3.12 Implement `GetMyWatchSpacesQuery` + handler
- [x] 3.13 Implement `GetWatchSpaceByIdQuery` + handler (validates caller is a member)
- [x] 3.14 Implement `ListInvitationsQuery` + handler (validates caller is `Owner`)
- [x] 3.15 Dispatch `MemberJoinedWatchSpace` and `MemberLeftWatchSpace` integration events after relevant commands commit

## 4. Contracts

- [x] 4.1 Define `MemberJoinedWatchSpace` integration event record (`WatchSpaceId`, `UserId`, `Role`, `JoinedAtUtc`)
- [x] 4.2 Define `MemberLeftWatchSpace` integration event record (`WatchSpaceId`, `UserId`, `LeftAtUtc`)

## 5. Infrastructure Layer

- [x] 5.1 Create `WatchSpacesDbContext` with `DbSet<WatchSpace>` and EF Core entity configurations
- [x] 5.2 Configure EF Core mappings for `WatchSpace`, `WatchSpaceMember`, `Invitation` targeting the `watch_spaces` schema
- [x] 5.3 Add unique constraint on `watch_space_members(watch_space_id, user_id)`
- [x] 5.4 Add unique constraint on `invitations.token`
- [x] 5.5 Implement `WatchSpaceRepository` using `WatchSpacesDbContext`
- [x] 5.6 Implement `UserReadModel` querying `identity.users` table (read-only cross-schema)
- [x] 5.7 Implement `NoOpInvitationEmailSender` (logs to `ILogger`, does not send real email)
- [x] 5.8 Add EF Core migration for `watch_spaces` schema tables
- [x] 5.9 Register all services in a `WatchSpacesModule.AddWatchSpaces(IServiceCollection)` extension method

## 6. API Endpoints

- [x] 6.1 Register `POST /watchspaces` → `CreateWatchSpaceCommand`
- [x] 6.2 Register `GET /watchspaces` → `GetMyWatchSpacesQuery`
- [x] 6.3 Register `GET /watchspaces/{id}` → `GetWatchSpaceByIdQuery`
- [x] 6.4 Register `PATCH /watchspaces/{id}` → `RenameWatchSpaceCommand`
- [x] 6.5 Register `POST /watchspaces/{id}/transfer-ownership` → `TransferOwnershipCommand`
- [x] 6.6 Register `POST /watchspaces/{id}/invitations` → `InviteMemberCommand`
- [x] 6.7 Register `GET /watchspaces/{id}/invitations` → `ListInvitationsQuery`
- [x] 6.8 Register `DELETE /watchspaces/{id}/invitations/{invitationId}` → `RevokeInvitationCommand`
- [x] 6.9 Register `POST /watchspaces/invitations/{token}/accept` → `AcceptInvitationCommand`
- [x] 6.10 Register `POST /watchspaces/invitations/{token}/decline` → `DeclineInvitationCommand`
- [x] 6.11 Register `DELETE /watchspaces/{id}/members/{userId}` → `RemoveMemberCommand`
- [x] 6.12 Register `DELETE /watchspaces/{id}/members/me` → `LeaveWatchSpaceCommand`
- [x] 6.13 Wire `services.AddWatchSpaces(...)` in `Program.cs`
- [x] 6.14 Apply `watch_spaces` migration in API startup / CI script

## 7. Integration Tests

- [x] 7.1 Test `POST /watchspaces` — create space, verify 201 and creator is `Owner`
- [x] 7.2 Test `GET /watchspaces` — returns only spaces the caller belongs to
- [x] 7.3 Test `GET /watchspaces/{id}` — returns details for member, 403 for non-member
- [x] 7.4 Test full invitation flow: invite → accept → member appears in space
- [x] 7.5 Test invitation decline and expiry rejection
- [x] 7.6 Test `RemoveMember` — success and sole-owner rejection
- [x] 7.7 Test `Leave` — success and sole-owner rejection
- [x] 7.8 Test `TransferOwnership` — roles swap correctly
- [x] 7.9 Verify `MemberJoinedWatchSpace` event is published on acceptance
- [x] 7.10 Verify `MemberLeftWatchSpace` event is published on removal/leave
