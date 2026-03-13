## Context

BloomWatch is a .NET 10 modular monolith. The Identity module currently exposes two endpoints (`POST /auth/register`, `POST /auth/login`) via Minimal API route groups in `IdentityEndpoints`. The `User` aggregate holds profile fields (Id, Email, DisplayName, AccountStatus, IsEmailVerified, CreatedAtUtc), and `IUserRepository` already provides `GetByIdAsync`. JWT middleware is configured and populates `ClaimsPrincipal` with a `sub` claim containing the UserId.

There is no endpoint for an authenticated user to retrieve their own profile. The registration handler returns `201 Created` with a location header pointing to `/users/{id}`, but that route does not resolve.

## Goals / Non-Goals

**Goals:**
- Provide a single authenticated endpoint for users to fetch their own profile
- Follow the existing Application layer pattern (command/query + handler)
- Reuse the existing repository and domain model — no new infrastructure

**Non-Goals:**
- Updating profile fields (display name, email, password) — future change
- Viewing other users' profiles — future change
- Caching or read-model projections — premature for a single-row lookup

## Decisions

### 1. Route: `GET /users/me` under Identity group

The endpoint will be `GET /users/me` rather than `GET /users/{id}`.

**Rationale**: `/users/me` is a well-established REST convention for "the currently authenticated user." It avoids exposing user IDs in URLs, prevents IDOR concerns, and aligns with the fact that we only support self-profile retrieval for now.

**Alternative considered**: `GET /auth/profile` — rejected because `/auth` is for authentication actions (register, login, token refresh), not resource retrieval. Placing it under `/users` signals it's a resource endpoint.

### 2. Extract UserId from JWT `sub` claim in the endpoint handler

The endpoint handler will parse `ClaimsPrincipal` to extract the `sub` claim, convert it to a `UserId`, and pass it to the query handler. This mirrors how protected endpoints typically work in ASP.NET Minimal APIs.

**Rationale**: Keeps the Application layer claim-agnostic — the query handler receives a typed `UserId`, not raw claims. The thin endpoint layer handles the HTTP-to-domain translation.

### 3. Query object + handler (not CQRS, just separation)

Introduce `GetUserProfileQuery` and `GetUserProfileQueryHandler` following the existing use-case pattern (similar to `RegisterUserCommand`/`RegisterUserCommandHandler`).

**Rationale**: Maintains consistency with the existing codebase conventions. Even though this is a simple lookup, the pattern keeps the Application layer as the single entry point for business logic.

### 4. Return a flat DTO, not the domain entity

The handler returns a `UserProfileResult` record with only the fields appropriate for API consumers: UserId, Email, DisplayName, AccountStatus, IsEmailVerified, CreatedAtUtc.

**Rationale**: Prevents leaking domain internals (e.g., PasswordHash) through the API response. The DTO is defined in the Application layer alongside the query.

## Risks / Trade-offs

- **[Risk] UserId claim missing or malformed in JWT** → Mitigation: The `[Authorize]` attribute ensures only valid JWTs reach the handler. If the `sub` claim is somehow missing, the endpoint returns 401 before the query handler is invoked.
- **[Risk] User deleted between token issuance and profile fetch** → Mitigation: The handler returns 404 if `GetByIdAsync` returns null. JWTs are short-lived (1h), limiting the window.
- **[Trade-off] No caching** → Accepted for now. A single-row primary-key lookup is fast. Caching can be added later if profile reads become a hot path.
