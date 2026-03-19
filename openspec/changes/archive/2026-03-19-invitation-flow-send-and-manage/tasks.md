## 1. Models and Service Layer

- [x] 1.1 Add `InvitationDetail` and `InviteMemberResponse` interfaces to `watch-space.model.ts`
- [x] 1.2 Add invitation service methods to `WatchSpaceService`: `sendInvitation`, `listInvitations`, `revokeInvitation`, `acceptInvitation`, `declineInvitation`
- [x] 1.3 Write unit tests for the new `WatchSpaceService` invitation methods

## 2. Invite Form (Owner-Only)

- [x] 2.1 Add invite form state signals to `WatchSpaceDetail` component (inviteEmail, isInviting, inviteError, inviteSuccess)
- [x] 2.2 Add invite form template section to `watch-space-detail.html` (email input + Invite button, gated by `isOwner()`)
- [x] 2.3 Implement `sendInvite()` method: validate email, call service, show success/error, clear input, refresh list
- [x] 2.4 Add SCSS styles for the invite form section

## 3. Pending Invitations List (Owner-Only)

- [x] 3.1 Add invitation list state signals to `WatchSpaceDetail` (invitations, isLoadingInvitations)
- [x] 3.2 Add `loadInvitations()` method that fetches from the list endpoint when the user is the owner
- [x] 3.3 Add invitations list template section to `watch-space-detail.html` (email, sent date, expiry, revoke button)
- [x] 3.4 Implement `revokeInvitation()` method with confirm() dialog and list refresh on success
- [x] 3.5 Add SCSS styles for the invitations list section

## 4. Accept/Decline Page

- [x] 4.1 Create `InvitationResponse` component at `features/watch-spaces/invitation-response.ts` with template and styles
- [x] 4.2 Add route `invitations/:token` to `watch-spaces.routes.ts`
- [x] 4.3 Implement token resolution: on init, attempt accept to determine if token is valid (or use a dedicated approach to show the page)
- [x] 4.4 Implement accept flow: call `acceptInvitation`, redirect to `/watch-spaces/{watchSpaceId}` on success
- [x] 4.5 Implement decline flow: call `declineInvitation`, show "Declined" confirmation message
- [x] 4.6 Implement error state handling: expired (410), forbidden (403), not found (404), already processed (409)
- [x] 4.7 Add SCSS styles for the invitation response page

## 5. Integration and Testing

- [x] 5.1 Write component tests for the invite form (owner visibility, submit, error states)
- [x] 5.2 Write component tests for the pending invitations list (load, revoke, empty state)
- [x] 5.3 Write component tests for the accept/decline page (accept, decline, error states)
- [ ] 5.4 Manual end-to-end verification of the full invitation flow
