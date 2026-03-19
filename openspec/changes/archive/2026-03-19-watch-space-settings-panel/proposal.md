## Why

Watch space owners currently have no way to manage their space from the frontend — renaming, viewing members, removing members, and transferring ownership all require direct API calls. Story 8.2 introduces a settings panel within the watch space detail view so owners can manage their space and all members can see who they're watching with.

## What Changes

- Build out the `WatchSpaceDetail` component (currently a placeholder) as the primary watch space view with a settings panel section
- Add TypeScript interfaces for `WatchSpaceDetail`, `MemberDetail` to the frontend models
- Extend `WatchSpaceService` with methods for: get by ID, rename, remove member, transfer ownership, and leave space
- Implement a settings panel accessible from the watch space detail view with:
  - Inline-editable space name (owner only)
  - Member list showing display name, role badge, and join date
  - Remove member action (owner only, per non-owner member)
  - Transfer ownership action with confirmation (owner only)
  - Leave space action for non-owner members
- Add `displayName` to the backend `MemberDetail` response (currently missing — the spec requires it but the implementation only returns `userId`, `role`, `joinedAt`)

## Capabilities

### New Capabilities

- `watch-space-settings-panel`: Frontend settings panel component within the watch space detail view — space renaming, member list with role display, member removal, ownership transfer, and leave actions with role-based visibility

### Modified Capabilities

- `watch-space-management`: The `GET /watchspaces/{id}` endpoint's `MemberDetail` response needs to include `displayName` (required by the existing spec but not yet implemented in the backend)
- `watch-space-selector`: `WatchSpaceService` needs additional methods (getById, rename, removeMember, transferOwnership, leaveSpace) beyond the current getMyWatchSpaces/createWatchSpace

## Impact

- **Frontend**: `watch-space-detail.ts` (full rewrite from placeholder), `watch-space.service.ts` (new methods), `watch-space.model.ts` (new interfaces)
- **Backend**: `GetWatchSpaceByIdQueryHandler` and `MemberDetail` DTO need `displayName` resolution via cross-module identity read
- **Shared UI**: Uses existing `bloom-button`, `bloom-card`, `bloom-badge`, `bloom-input` components
- **No new dependencies**: All backend endpoints already exist; this is a frontend feature with one backend DTO enhancement
