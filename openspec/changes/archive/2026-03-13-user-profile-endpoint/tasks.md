## 1. Application Layer

- [x] 1.1 Create `UserProfileResult` record in `UseCases/GetProfile/` with fields: UserId, Email, DisplayName, AccountStatus, IsEmailVerified, CreatedAtUtc
- [x] 1.2 Create `GetUserProfileQuery` record accepting a `UserId` parameter
- [x] 1.3 Create `GetUserProfileQueryHandler` that uses `IUserRepository.GetByIdAsync` to load the user and map to `UserProfileResult`, throwing if the user is not found

## 2. API Endpoint

- [x] 2.1 Add `GET /users/me` endpoint method in `IdentityEndpoints` that extracts the `sub` claim from `ClaimsPrincipal`, converts to `UserId`, and delegates to the query handler
- [x] 2.2 Register the endpoint with `[Authorize]`, OpenAPI metadata (summary, description, produces 200/401/404), and route name

## 3. Tests

- [x] 3.1 Add unit tests for `GetUserProfileQueryHandler`: success case returns mapped DTO, user-not-found case throws
- [x] 3.2 Add integration tests for `GET /users/me`: authenticated returns 200 with correct fields, unauthenticated returns 401, nonexistent user returns 404
