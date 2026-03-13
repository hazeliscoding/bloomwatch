## Why

Authenticated users currently have no way to retrieve their own profile information after login. The JWT contains basic claims, but there is no API endpoint to fetch the full, authoritative user profile from the server. This is needed for client applications to display user details (profile pages, settings screens) and to verify account state (e.g., email verification status).

## What Changes

- Add a new authenticated `GET /users/me` endpoint that returns the current user's profile based on their JWT claims
- Add a `GetUserProfile` query/handler in the Identity Application layer to encapsulate the read operation
- Wire the new endpoint into the existing `IdentityEndpoints` route group with proper `[Authorize]` protection

## Capabilities

### New Capabilities
- `user-profile`: Authenticated users can retrieve their own profile information via a protected API endpoint

### Modified Capabilities
_None — this change introduces a read-only endpoint using existing domain entities and repository methods._

## Impact

- **Identity module (Application layer)**: New `GetUserProfile` use case (query + handler)
- **API host**: New endpoint registered in `IdentityEndpoints`
- **Existing code**: Leverages `IUserRepository.GetByIdAsync` — no domain or infrastructure changes required
- **Database**: No schema changes — reads from the existing `identity.users` table
- **Authentication**: Endpoint requires a valid JWT; relies on the existing JWT middleware to populate `ClaimsPrincipal`
