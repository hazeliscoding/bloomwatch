## 1. Solution Scaffold

- [x] 1.1 Create `BloomWatch.Modules.Identity.Domain` class library project
- [x] 1.2 Create `BloomWatch.Modules.Identity.Application` class library project
- [x] 1.3 Create `BloomWatch.Modules.Identity.Infrastructure` class library project
- [x] 1.4 Create `BloomWatch.Modules.Identity.Contracts` class library project
- [x] 1.5 Add project references: Application → Domain, Infrastructure → Application + Domain, Contracts → Domain
- [x] 1.6 Add all four Identity projects to the solution file

## 2. Domain Layer — User Aggregate

- [x] 2.1 Define `UserId` value object (strongly-typed wrapper over `Guid`)
- [x] 2.2 Define `EmailAddress` value object with RFC 5321 format validation
- [x] 2.3 Define `DisplayName` value object with non-empty/whitespace validation
- [x] 2.4 Define `AccountStatus` enum (`Active`, `Suspended`)
- [x] 2.5 Implement `User` aggregate root with fields: `UserId`, `EmailAddress`, `DisplayName`, `PasswordHash`, `AccountStatus`, `IsEmailVerified`, `CreatedAtUtc`
- [x] 2.6 Add `User.Register(email, passwordHash, displayName)` static factory method that sets `AccountStatus = Active` and `IsEmailVerified = false`
- [x] 2.7 Add `IUserRepository` interface to the Domain layer (`GetByIdAsync`, `GetByEmailAsync`, `AddAsync`, `ExistsWithEmailAsync`)

## 3. Application Layer — Interfaces and Use Cases

- [x] 3.1 Define `IPasswordHasher` interface (`Hash(plainText)` and `Verify(plainText, hash)`) in the Application layer
- [x] 3.2 Define `IJwtTokenGenerator` interface (`GenerateToken(user): TokenResult`) in the Application layer
- [x] 3.3 Implement `RegisterUserCommand` record and `RegisterUserCommandHandler` that: validates uniqueness via `IUserRepository`, hashes password via `IPasswordHasher`, calls `User.Register`, persists via `IUserRepository`, returns the created user's ID + email + display name
- [x] 3.4 Implement `LoginUserCommand` record and `LoginUserCommandHandler` that: looks up user by email, verifies account is `Active`, verifies password via `IPasswordHasher`, calls `IJwtTokenGenerator`, returns `accessToken` + `expiresAt`

## 4. Infrastructure Layer

- [x] 4.1 Add NuGet packages: `Microsoft.EntityFrameworkCore`, `Npgsql.EntityFrameworkCore.PostgreSQL`, `BCrypt.Net-Next`, `Microsoft.AspNetCore.Authentication.JwtBearer`, `System.IdentityModel.Tokens.Jwt`
- [x] 4.2 Create `IdentityDbContext` with `HasDefaultSchema("identity")` and a `Users` `DbSet<User>`
- [x] 4.3 Configure `UserConfiguration` (EF Core `IEntityTypeConfiguration<User>`) mapping all aggregate fields; set table name `users`, primary key `user_id`, unique index on `email`
- [x] 4.4 Implement `EfUserRepository` satisfying `IUserRepository` using `IdentityDbContext`
- [x] 4.5 Implement `BcryptPasswordHasher` satisfying `IPasswordHasher` using `BCrypt.Net-Next` with work factor 12
- [x] 4.6 Implement `JwtTokenGenerator` satisfying `IJwtTokenGenerator`; read secret key + issuer + audience from `IConfiguration`; include `sub`, `email`, `display_name`, `iat`, `exp` claims; sign with HS256; set 1-hour expiry
- [x] 4.7 Create `AddIdentityModule(IServiceCollection, IConfiguration)` extension method that registers `IdentityDbContext`, `IUserRepository`, `IPasswordHasher`, `IJwtTokenGenerator`, and JWT bearer authentication middleware
- [x] 4.8 Add EF Core migration `InitialIdentitySchema` creating `identity.users` table

## 5. Contracts Layer

- [x] 5.1 Define `UserRegisteredIntegrationEvent` record (`UserId`, `Email`, `DisplayName`, `CreatedAtUtc`) for future cross-module consumption

## 6. API Layer — Endpoints

- [x] 6.1 Create `IdentityController` (or minimal-API endpoint group) under `/auth`
- [x] 6.2 Implement `POST /auth/register` endpoint: maps request → `RegisterUserCommand`, returns 201 with user profile on success, 409 on duplicate email, 400 on validation errors
- [x] 6.3 Implement `POST /auth/login` endpoint: maps request → `LoginUserCommand`, returns 200 with `{ accessToken, expiresAt }` on success, 401 on invalid credentials or inactive account
- [x] 6.4 Wire `AddIdentityModule(...)` into `Program.cs`
- [x] 6.5 Configure JWT bearer authentication in `Program.cs` (or delegate fully to the module extension method)

## 7. Tests

- [x] 7.1 Unit-test `EmailAddress` value object: valid email passes, invalid format throws
- [x] 7.2 Unit-test `DisplayName` value object: non-empty passes, blank/whitespace throws
- [x] 7.3 Unit-test `User.Register` factory: aggregate fields set correctly
- [x] 7.4 Unit-test `RegisterUserCommandHandler`: duplicate email returns error, valid input creates and persists user
- [x] 7.5 Unit-test `LoginUserCommandHandler`: wrong password returns 401-equivalent error, inactive account returns 401-equivalent error, valid credentials return token result
- [x] 7.6 Integration-test `POST /auth/register`: happy path returns 201; duplicate email returns 409
- [x] 7.7 Integration-test `POST /auth/login`: valid credentials return 200 with token; bad password returns 401
- [x] 7.8 Integration-test JWT middleware: protected dummy route returns 401 with no token, 200 with valid token
