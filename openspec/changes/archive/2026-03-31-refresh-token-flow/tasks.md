## 1. Domain Layer (Identity)

- [x] 1.1 Create `RefreshToken` aggregate in `BloomWatch.Modules.Identity.Domain/Aggregates/` with properties: `Id` (Guid), `UserId`, `TokenHash` (string), `ExpiresAt` (DateTime), `CreatedAt` (DateTime), `IsRevoked` (bool), and an `IsValid()` method
- [x] 1.2 Create `IRefreshTokenRepository` interface in `BloomWatch.Modules.Identity.Domain/Repositories/` with methods: `AddAsync`, `GetByHashAsync`, `RevokeAsync`

## 2. Application Layer (Identity)

- [x] 2.1 Add `IRefreshTokenService` abstraction in `BloomWatch.Modules.Identity.Application/Abstractions/` for generating and hashing opaque tokens (32-byte random, base64url, SHA-256 hash)
- [x] 2.2 Update `LoginUserCommand` and `LoginUserCommandHandler` to call `IRefreshTokenRepository.AddAsync` and return `RefreshToken` + `RefreshTokenExpiresAt` in `LoginUserResult`
- [x] 2.3 Create `RefreshTokenCommand` and `RefreshTokenCommandHandler` in `UseCases/RefreshToken/` — validates token via hash lookup, checks `IsValid()`, revokes old token, issues new access + refresh token pair
- [x] 2.4 Create `RevokeTokenCommand` and `RevokeTokenCommandHandler` in `UseCases/RevokeToken/` — looks up token by hash, calls `RevokeAsync` (idempotent; no error if not found)

## 3. Infrastructure Layer (Identity)

- [x] 3.1 Implement `RefreshTokenService` in `BloomWatch.Modules.Identity.Infrastructure/Auth/` — `GenerateToken()` returns a 32-byte base64url string; `HashToken(string token)` returns its SHA-256 hex digest
- [x] 3.2 Add `RefreshToken` EF Core entity configuration in `Persistence/Configurations/RefreshTokenConfiguration.cs` — table `refresh_tokens`, schema `identity`; index on `TokenHash`
- [x] 3.3 Add `DbSet<RefreshToken> RefreshTokens` to `IdentityDbContext`
- [x] 3.4 Implement `EfRefreshTokenRepository` in `Persistence/Repositories/` implementing `IRefreshTokenRepository`
- [x] 3.5 Add EF Core migration: `dotnet ef migrations add AddRefreshTokens --project ... --startup-project ... --context IdentityDbContext`
- [x] 3.6 Register `IRefreshTokenRepository` and `IRefreshTokenService` in `Extensions/ServiceCollectionExtensions.cs`

## 4. API Layer

- [x] 4.1 Add `POST /auth/refresh` endpoint in `IdentityEndpoints.cs` — calls `RefreshTokenCommandHandler`, returns `{ accessToken, expiresAt, refreshToken, refreshTokenExpiresAt }` or 401
- [x] 4.2 Add `POST /auth/revoke` endpoint in `IdentityEndpoints.cs` — calls `RevokeTokenCommandHandler`, returns 204 always
- [x] 4.3 Update `POST /auth/login` response in `IdentityEndpoints.cs` to include `refreshToken` and `refreshTokenExpiresAt`

## 5. Backend Tests

- [x] 5.1 Unit tests for `RefreshToken.IsValid()` — revoked, expired, and valid cases (`BloomWatch.Modules.Identity.UnitTests`)
- [x] 5.2 Unit tests for `RefreshTokenCommandHandler` — valid rotation, expired token, revoked token, unknown token (`BloomWatch.Modules.Identity.UnitTests`)
- [x] 5.3 Unit tests for `RevokeTokenCommandHandler` — successful revoke, unknown token idempotency
- [x] 5.4 Integration tests for `POST /auth/refresh` and `POST /auth/revoke` in `BloomWatch.Modules.Identity.IntegrationTests`
- [x] 5.5 Update `POST /auth/login` integration test to assert the response now includes `refreshToken` and `refreshTokenExpiresAt`

## 6. Frontend — AuthService

- [x] 6.1 Update `AuthService` in `core/auth/auth.service.ts`: replace `setToken()` with `setTokens(accessToken, expiresAt, refreshToken, refreshTokenExpiresAt)`, update `clearTokens()` to also remove `bloom_refresh_token` and `bloom_refresh_token_expires_at` from `localStorage`
- [x] 6.2 Add `refreshToken` signal to `AuthService` (read from `localStorage`), update `isAuthenticated` computed signal to return `true` when access token is expired but refresh token is valid
- [x] 6.3 Add `refreshAccessToken()` method to `AuthService` that calls `POST /auth/refresh`, calls `setTokens()` with the new pair, and returns an `Observable<string>` (new access token)

## 7. Frontend — Auth Interceptor

- [x] 7.1 Update `authInterceptor` in `core/http/auth.interceptor.ts`: before sending a request, if `isAuthenticated()` is true but access token is expired and a refresh token exists, call `authService.refreshAccessToken()` first (proactive refresh)
- [x] 7.2 Add 401 fallback to `authInterceptor`: on 401 response, attempt one refresh-and-retry; on second failure, call `authService.clearTokens()` and navigate to `/auth/login`
- [x] 7.3 Add deduplication to the refresh call in the interceptor (use a shared `refreshInProgress$` observable or `BehaviorSubject` flag so concurrent requests share one refresh)

## 8. Frontend — Login Flow Update

- [x] 8.1 Update the login handler in `features/auth` (wherever `LoginUserResult` is consumed) to call `authService.setTokens(...)` with all four fields from the API response
- [x] 8.2 Update Vitest unit tests for `AuthService` to cover the new `setTokens`, `clearTokens`, and `refreshToken` signal behaviors

## 9. Documentation

- [x] 9.1 Update `appsettings.json` / `appsettings.Development.json` comments (if any) to note that no new config is needed for refresh tokens
- [x] 9.2 Run `./scripts/apply-migrations.sh` and verify the migration applies cleanly in local dev
