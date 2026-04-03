## 1. Domain Layer (Identity)

- [x] 1.1 Create `PasswordResetToken` entity in `BloomWatch.Modules.Identity.Domain/Aggregates/` with `Id`, `UserId`, `TokenHash`, `ExpiresAt`, `CreatedAt`, `IsUsed`; add `Create()` factory and `IsValid()` method
- [x] 1.2 Create `IPasswordResetTokenRepository` interface in `BloomWatch.Modules.Identity.Domain/Repositories/` with `AddAsync`, `FindByHashAsync`, and `SaveChangesAsync` methods

## 2. Infrastructure Layer (Identity)

- [x] 2.1 Create `PasswordResetTokenConfiguration` in `Infrastructure/Persistence/Configurations/` (table name `password_reset_tokens`, index on `TokenHash`, index on `ExpiresAt`)
- [x] 2.2 Register `PasswordResetTokens` DbSet in `IdentityDbContext` and apply the configuration
- [x] 2.3 Implement `PasswordResetTokenRepository` in `Infrastructure/Persistence/Repositories/`
- [x] 2.4 Register `IPasswordResetTokenRepository` → `PasswordResetTokenRepository` in `ServiceCollectionExtensions.cs`
- [x] 2.5 Add EF Core migration `AddPasswordResetTokensTable` to the Identity module

## 3. Application Layer (Identity) — ForgotPassword

- [x] 3.1 Create `ForgotPasswordCommand` record (`Email`) and `ForgotPasswordCommandHandler` in `Application/`
- [x] 3.2 Handler logic: look up user by email (return silently if not found or not Active), generate 32-byte random token, compute SHA-256 hash, persist `PasswordResetToken`, call `IEmailSender.SendAsync` with HTML + plain-text reset email containing `<AppBaseUrl>/auth/reset-password?token=<plainToken>`
- [x] 3.3 Read `AppBaseUrl` from configuration (`App:BaseUrl`) in the handler

## 4. Application Layer (Identity) — ResetPassword

- [x] 4.1 Create `ResetPasswordCommand` record (`Token`, `NewPassword`) and `ResetPasswordCommandHandler` in `Application/`
- [x] 4.2 Handler logic: compute SHA-256 hash of submitted token, look up `PasswordResetToken` by hash; return `InvalidToken` result if not found; check `IsValid()` — return `Expired` or `AlreadyUsed` result as appropriate; validate password strength; hash new password with bcrypt; update `User.PasswordHash`; mark token `IsUsed = true`; save both changes atomically

## 5. API Layer (Identity)

- [x] 5.1 Register `POST /auth/forgot-password` in `IdentityEndpoints.cs`: parse request, run `ForgotPasswordCommandHandler`, return `200 OK` with generic message always
- [x] 5.2 Register `POST /auth/reset-password` in `IdentityEndpoints.cs`: parse request, run `ResetPasswordCommandHandler`, map `InvalidToken`/`Expired`/`AlreadyUsed` results to `400 Bad Request` with appropriate messages, return `200 OK` on success
- [x] 5.3 Add fixed-window rate-limiter policy (`forgot-password-limit`: 5 req / IP / 60 min) in `Program.cs` and apply it to the `forgot-password` endpoint

## 6. Backend Tests

- [x] 6.1 Unit tests for `PasswordResetToken.IsValid()` (unused+non-expired = true, used = false, expired = false) — `BloomWatch.Modules.Identity.UnitTests`
- [x] 6.2 Unit tests for `ForgotPasswordCommandHandler`: unknown email returns silently, inactive account returns silently, active account creates token and calls `IEmailSender`
- [x] 6.3 Unit tests for `ResetPasswordCommandHandler`: invalid token → `InvalidToken`, expired token → `Expired`, used token → `AlreadyUsed`, weak password → validation error, valid → updates password and marks token used
- [x] 6.4 Integration test for `POST /auth/forgot-password`: valid email → 200 with generic message; `NullEmailSender` captures call
- [x] 6.5 Integration test for `POST /auth/reset-password`: expired token → 400; used token → 400; valid token + password → 200; subsequent use of same token → 400

## 7. Frontend — AuthService

- [x] 7.1 Add `forgotPassword(email: string): Observable<void>` to `AuthService` delegating to `ApiService.post('/auth/forgot-password', { email })`
- [x] 7.2 Add `resetPassword(token: string, newPassword: string): Observable<void>` to `AuthService` delegating to `ApiService.post('/auth/reset-password', { token, newPassword })`

## 8. Frontend — ForgotPasswordComponent

- [x] 8.1 Create `ForgotPasswordComponent` standalone component in `features/auth/forgot-password/` using `MinimalLayout`
- [x] 8.2 Build the form: email input (`bloom-input`), submit button (`bloom-button`), inline validation error, loading state during request
- [x] 8.3 On successful submission (or any response): replace form with confirmation message "If that email is registered, we've sent a reset link. Check your inbox." and "Back to login" link
- [x] 8.4 Add "Back to login" link visible before and after submission
- [x] 8.5 Register route `/auth/forgot-password` in the auth feature routes

## 9. Frontend — ResetPasswordComponent

- [x] 9.1 Create `ResetPasswordComponent` standalone component in `features/auth/reset-password/` using `MinimalLayout`
- [x] 9.2 On init, read `token` query param; if missing, show error state ("Invalid or missing reset link.") without rendering the form
- [x] 9.3 Build the form: new-password input, confirm-password input (`bloom-input`), submit button (`bloom-button`), passwords-must-match inline validation, loading state
- [x] 9.4 On API success (`200`): replace form with "Your password has been reset!" message and "Go to login" link
- [x] 9.5 On API error (`400`): display server error message below the form (expired / already used / invalid)
- [x] 9.6 Register route `/auth/reset-password` in the auth feature routes

## 10. Frontend — Login Page Update

- [x] 10.1 Add "Forgot password?" link below the submit button in `LoginComponent`, navigating to `/auth/forgot-password`

## 11. Configuration

- [x] 11.1 Add `App:BaseUrl` key to `appsettings.json` (e.g., `http://localhost:4200`) and document it in the local dev setup notes
