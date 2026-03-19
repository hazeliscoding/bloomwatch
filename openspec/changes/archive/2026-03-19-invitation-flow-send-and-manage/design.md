## Context

The backend invitation system (Stories 2.6-2.10) is fully implemented with five endpoints:

- `POST /watchspaces/{id}/invitations` — send invite (owner only)
- `GET /watchspaces/{id}/invitations` — list invitations (owner only)
- `DELETE /watchspaces/{id}/invitations/{invitationId}` — revoke (owner only)
- `POST /watchspaces/invitations/{token}/accept` — accept (invitee)
- `POST /watchspaces/invitations/{token}/decline` — decline (invitee)

The Angular frontend has a `WatchSpaceDetail` component with a settings panel (rename, members, leave). No invitation UI exists yet. The `WatchSpaceService` currently has 7 methods but none for invitations.

## Goals / Non-Goals

**Goals:**
- Owner can send invitations, view pending list, and revoke from within the settings panel
- Invitee can accept/decline via a dedicated token-based page
- All error states (expired, invalid, duplicate, mismatch) show user-friendly feedback
- Follow existing Angular patterns: signals, `ApiService`, component-per-file convention

**Non-Goals:**
- Email notifications (backend has a no-op sender; real email is a separate concern)
- Bulk invitations or CSV import
- Invitation history beyond what the list endpoint returns
- Push/real-time updates when an invitation is accepted

## Decisions

### 1. Invitation management lives inside `WatchSpaceDetail`, not a separate component

**Rationale:** The invite form and pending list are owner-only sections within the existing settings panel. Extracting them to a routed sub-component adds routing complexity for a section that shares the same `detail` signal and `isOwner` guard. Instead, add them as template sections gated by `@if (isOwner())`.

**Alternative considered:** Separate `InvitationManager` component loaded via a child route. Rejected — the settings panel is already a single component and the invitation section is tightly coupled to the watch space context (space ID, owner check).

### 2. Accept/decline is a standalone routed page

**Rationale:** Invitees arrive via a link containing a token. They may not be on the watch space detail page (they may not even be a member yet). A dedicated route `/watch-spaces/invitations/:token` renders a self-contained page that loads the invitation context and presents accept/decline actions.

**Alternative considered:** Modal overlay on the watch space detail page. Rejected — invitees don't have access to the space detail until they accept.

### 3. Service methods added to existing `WatchSpaceService`

**Rationale:** Invitations are a sub-resource of watch spaces. All other watch space operations live in `WatchSpaceService`. Splitting into a separate `InvitationService` creates unnecessary indirection for five methods that share the same API base path.

### 4. Models added to existing `watch-space.model.ts`

**Rationale:** Same reasoning — invitation DTOs (`InvitationDetail`, `InviteMemberResponse`) are part of the watch space domain. Keeping them co-located avoids a separate model file for two interfaces.

### 5. Confirmation step for revoke uses `confirm()` dialog

**Rationale:** Consistent with existing patterns in the codebase (member removal, ownership transfer, leave space all use `confirm()`). A custom modal would be inconsistent and overengineered for MVP.

## Risks / Trade-offs

- **Token exposure in URL** → The accept/decline route includes the token in the URL path. This is standard practice for invitation links and matches the backend design. Tokens are single-use and expire in 7 days.
- **No optimistic UI for invite send** → After sending an invite, we refetch the invitation list rather than optimistically appending. This avoids stale data if the backend rejects (duplicate, already member). Trade-off: one extra API call per successful invite.
- **Settings panel component grows** → Adding invitation state (invite email, invite loading, invitations list, etc.) increases the signal count in `WatchSpaceDetail`. Acceptable for MVP; if the component becomes unwieldy in future, sections can be extracted into child components.
