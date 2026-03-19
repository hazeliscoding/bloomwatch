## Why

Story 8.3 requires the Angular frontend for the full invitation lifecycle: sending invitations, listing pending ones, revoking them (owner), and accepting/declining them (invitee). The backend endpoints (Stories 2.6-2.10) are fully implemented but there is zero frontend code wiring into them. Without this, owners cannot grow their watch space membership through the UI.

## What Changes

- Add invitation service methods to `WatchSpaceService` (send, list, revoke, accept, decline)
- Add invitation TypeScript models (`InvitationDetail`, `InviteMemberResult`)
- Build an **Invite Member** form inside the watch space settings panel (owner-only, email input)
- Build a **Pending Invitations** list inside the settings panel (owner-only, with revoke action)
- Add an **Invitation Accept/Decline** page at a token-based route for invitees
- Handle error states: expired token, invalid token, already-processed invitation, user mismatch

## Capabilities

### New Capabilities
- `invitation-send-form`: Owner-only email invite form with success/error feedback, integrated into the settings panel
- `invitation-list-manage`: Owner-only pending invitations list with revoke action and confirmation step
- `invitation-accept-decline`: Token-based accept/decline page for invitees with error handling for expired, invalid, or already-processed tokens

### Modified Capabilities
- `watch-space-settings-panel`: Settings panel gains an invitation section (invite form + pending list) visible only to owners

## Impact

- **Frontend routes**: New route `invitations/:token` under watch-spaces for the accept/decline page
- **Angular service**: `WatchSpaceService` gains 5 new methods for invitation API calls
- **Models**: New interfaces in `watch-space.model.ts` for invitation DTOs
- **Settings panel**: Template and component grow to include the invitation section
- **Backend**: No changes needed — all endpoints already exist
