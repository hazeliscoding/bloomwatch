## Context

The `WatchSpaceDetail` component at `/watch-spaces/:id` is currently a placeholder stub. All backend endpoints for watch space management are fully implemented (rename, get by ID with members, remove member, transfer ownership, leave). The frontend has only two service methods (`getMyWatchSpaces`, `createWatchSpace`) and two model interfaces (`WatchSpaceSummary`, `CreateWatchSpaceRequest`).

The frontend follows Angular 21 patterns: standalone components, signals for state, `inject()` for DI, new control flow syntax (`@if`, `@for`), and a shared UI component library (`bloom-*`). The SCSS design system uses a kawaii/Y2K theme with CSS custom properties.

One backend gap exists: `MemberDetail` in the `GET /watchspaces/{id}` response returns `userId`, `role`, `joinedAt` but not `displayName`. The settings panel needs display names to be usable.

## Goals / Non-Goals

**Goals:**

- Replace the `WatchSpaceDetail` placeholder with a functional detail page that includes a settings panel
- Show the member list with display names, roles, and join dates
- Enable owners to rename the space, remove members, and transfer ownership — all from the UI
- Enable non-owner members to leave the space
- Resolve the missing `displayName` in the backend `MemberDetail` DTO
- Follow existing frontend patterns (signals, standalone, bloom-* UI components)

**Non-Goals:**

- Invitation management (send, revoke, list pending) — that's Story 8.3, a separate change
- Watch space deletion — not in the MVP
- Real-time member list updates (WebSocket/SSE) — standard request/response is sufficient
- Dashboard/anime list within the detail view — separate stories handle those views

## Decisions

### 1. Single-file component structure for the detail page

**Decision:** Build `WatchSpaceDetail` as a single component with inline sections for general info and settings, rather than extracting sub-components into separate files.

**Rationale:** The settings panel is relatively contained (name edit, member list, a few action buttons). Extracting into sub-components would add file overhead for a page that fits within a single component. The existing `WatchSpaceList` component follows a similar pattern — one component file with template and styles alongside it.

**Alternative considered:** Extract `MemberListSection`, `RenameSection`, etc. as separate components. Rejected because the interaction between sections is minimal and doesn't warrant the abstraction overhead at this point.

### 2. Add `displayName` to backend `MemberDetail` via cross-module identity read

**Decision:** Extend `GetWatchSpaceByIdQueryHandler` to resolve display names by batch-querying the Identity schema using an `IUserDisplayNameLookup` abstraction (the same pattern used by the Analytics module for rating gap highlights).

**Rationale:** The WatchSpaces module already has a cross-module `IdentityReadDbContext` for email lookups. Adding a display name lookup follows the established cross-module read pattern. A single batch query for all member user IDs is efficient.

**Alternative considered:** Resolve display names on the frontend by adding a `/users/{id}/profile` endpoint. Rejected because it would require N+1 requests for N members and add unnecessary API surface.

### 3. Confirmation dialogs for destructive actions using native `confirm()`

**Decision:** Use the browser's native `window.confirm()` for "Remove member" and "Transfer ownership" confirmation prompts.

**Rationale:** These are infrequent, high-stakes actions where a simple confirmation is appropriate. A custom modal component would be over-engineered for two confirm dialogs. This matches the simplicity principle of the MVP.

**Alternative considered:** Build a custom `bloom-dialog` component. Rejected as premature — can be added later when more dialogs are needed across the app.

### 4. Inline edit for space name

**Decision:** Implement a click-to-edit pattern for the space name: display the name as text, and when the owner clicks an edit icon, switch to an input field with save/cancel actions.

**Rationale:** This is the standard UX for editable labels and matches the acceptance criteria ("inline edit control"). It avoids navigating to a separate edit form for a single field change.

### 5. Role-based visibility via `computed()` signals

**Decision:** Compute `isOwner` as a `computed()` signal derived from the current user's ID and the member list. Use this signal to conditionally render owner-only actions in the template with `@if (isOwner())`.

**Rationale:** Signals propagate reactivity automatically when the member list updates (e.g., after a transfer). This avoids manual state synchronization and follows the project's signal-first pattern.

### 6. Optimistic UI updates after mutations

**Decision:** After a successful rename, remove, transfer, or leave API call, update the local state directly rather than re-fetching the entire watch space detail.

**Rationale:** The mutations have predictable outcomes (a member disappears, a name changes, roles swap). Optimistic updates feel snappier and avoid an extra round-trip. If the API call fails, the error is shown and no state update occurs.

**Alternative considered:** Always re-fetch after mutation. Acceptable but slower — only needed if the server adds computed fields the client can't predict.

## Risks / Trade-offs

- **[Risk] Display name resolution adds a cross-module query to `GetWatchSpaceById`** → Acceptable: one batch query for typically <20 user IDs. The Analytics module does the same for rating gaps.
- **[Risk] Ownership transfer updates the current user's `isOwner` state** → Handled: after successful transfer, the local member list is updated and `isOwner` recomputes to `false` automatically via signals.
- **[Risk] Native `confirm()` may feel inconsistent with the kawaii theme** → Acceptable trade-off for MVP. A custom dialog can replace it later without changing the component logic.
- **[Risk] Race condition if two owners act simultaneously (e.g., both try to remove the same member)** → The backend handles this with domain-level validation. The frontend shows the error response and the user can refresh.
