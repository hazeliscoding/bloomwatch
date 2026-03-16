## 1. Environment Configuration

- [x] 1.1 Create `src/BloomWatch.UI/src/environments/environment.ts` with `apiBaseUrl` placeholder for production
- [x] 1.2 Create `src/BloomWatch.UI/src/environments/environment.development.ts` with `apiBaseUrl` pointing to local dev server
- [x] 1.3 Configure `fileReplacements` in `angular.json` so development builds use `environment.development.ts`

## 2. AuthService — Token Management

- [x] 2.1 Create `src/BloomWatch.UI/src/app/core/auth/auth.service.ts` with `setToken()`, `clearToken()`, localStorage persistence under `bloom_access_token` and `bloom_token_expires_at`
- [x] 2.2 Expose `token` and `isAuthenticated` signals (computed from token presence and expiry)
- [x] 2.3 Initialize auth state from localStorage on construction (survive page reloads)

## 3. Auth Interceptor

- [x] 3.1 Create `src/BloomWatch.UI/src/app/core/http/auth.interceptor.ts` as a functional `HttpInterceptorFn`
- [x] 3.2 Attach `Authorization: Bearer <token>` header when token is available
- [x] 3.3 Skip token attachment for requests to `/auth/*` paths
- [x] 3.4 Handle 401 responses: call `clearToken()` and redirect to `/login`

## 4. ApiService — Typed HTTP Wrapper

- [x] 4.1 Create `src/BloomWatch.UI/src/app/core/http/api.service.ts` with typed `get<T>()`, `post<T>()`, `put<T>()`, `patch<T>()`, `delete<T>()` methods
- [x] 4.2 Auto-prefix all request URLs with `environment.apiBaseUrl`

## 5. App Configuration Wiring

- [x] 5.1 Add `provideHttpClient(withInterceptors([authInterceptor]))` to `app.config.ts` providers
- [x] 5.2 Verify `HttpClient` is injectable and interceptor pipeline is active

## 6. Testing

- [x] 6.1 Write unit tests for `AuthService` (setToken, clearToken, signal reactivity, expiry detection, localStorage init)
- [x] 6.2 Write unit tests for `authInterceptor` (token attachment, auth-path skipping, 401 handling)
- [x] 6.3 Write unit tests for `ApiService` (URL prefixing, typed method delegation)
