## Context

BloomWatch is a .NET modular monolith backed by PostgreSQL. The Identity module is the first to be implemented and has no upstream dependencies — all other modules depend on it to resolve user identity. The module follows the DDD layering convention already established in the architecture: Domain → Application → Infrastructure → Contracts, living under `Modules/Identity/`.

No authentication infrastructure exists yet. There is no `users` table, no password hashing, and no token issuance.

## Goals / Non-Goals

**Goals:**
- Scaffold the `BloomWatch.Modules.Identity.*` project set with correct DDD layering
- Implement a `User` aggregate with email, hashed password, display name, and account status
- Implement `RegisterUser` and `LoginUser` use cases as application services / command handlers
- Expose `POST /auth/register` and `POST /auth/login` HTTP endpoints in `BloomWatch.Api`
- Issue a signed JWT access token on successful login
- Add JWT bearer middleware to `BloomWatch.Api` for protecting future routes

**Non-Goals:**
- Refresh tokens or token rotation (can be added later)
- Email verification flow
- Password reset / forgot-password flow
- AniList account linking (separate capability)
- Role-based authorization beyond the token being valid

## Decisions

### 1. JWT with symmetric signing (HMAC-SHA256) over asymmetric (RS256)

**Decision**: Use `HS256` with a secret key stored in configuration.

**Rationale**: BloomWatch is a single-service modular monolith. There is no token-consuming third party that needs to verify the signature independently, so the operational complexity of managing RSA key pairs is not justified at this stage.

**Alternative considered**: RS256 — rejected because it adds key-pair rotation complexity with no benefit for a monolith where the issuer and verifier are the same process.

### 2. BCrypt for password hashing via `BCrypt.Net-Next`

**Decision**: Use `BCrypt.Net-Next` (work factor 12) for password hashing.

**Rationale**: BCrypt is battle-tested, has a built-in cost factor, and is appropriate for password storage. The .NET ecosystem has a well-maintained wrapper.

**Alternative considered**: `Microsoft.AspNetCore.Identity`'s `PasswordHasher<T>` — rejected to avoid coupling to ASP.NET Identity's opinionated user management layer, which conflicts with our DDD `User` aggregate ownership.

### 3. Password hashing in the Domain, token issuance in Infrastructure

**Decision**: The `User` aggregate exposes a `Register(email, hashedPassword, displayName)` factory and a `ValidatePassword(plainText, hashedPassword)` static helper. The `IPasswordHasher` and `IJwtTokenGenerator` interfaces are defined in the Application layer and implemented in Infrastructure.

**Rationale**: Keeps the domain model free of I/O while still making password validation a first-class domain concern through interfaces.

### 4. `identity` PostgreSQL schema via EF Core

**Decision**: All Identity tables live in the `identity` schema, configured through EF Core's `HasDefaultSchema("identity")` in `IdentityDbContext`.

**Rationale**: Consistent with the architecture document's schema-per-module strategy. Migrations are scoped to the Identity infrastructure project.

### 5. Module registration via extension method

**Decision**: Expose a `AddIdentityModule(this IServiceCollection services, IConfiguration config)` extension method from `BloomWatch.Modules.Identity.Infrastructure` so `BloomWatch.Api` can register the module cleanly.

**Rationale**: Standard .NET modular monolith convention; keeps `Program.cs` thin.

## Risks / Trade-offs

- **No refresh tokens** → access tokens must have a reasonable expiry (e.g., 1 hour). Users will need to re-login when the token expires. Acceptable for MVP; refresh token support can be layered on later without breaking changes to the `User` aggregate.
- **HS256 shared secret** → if the secret is leaked, all tokens are compromised. Mitigated by keeping the secret in environment variables / secrets management and rotating on suspicion.
- **BCrypt is slow by design** → at work factor 12, hashing takes ~250 ms. This is acceptable for auth endpoints but login should never be on a hot path. No caching of hashed passwords.
- **No email verification** → a user can register with any email. This is a known gap for MVP; a `User.IsEmailVerified` flag is included in the aggregate so verification can be added without schema migration later.
