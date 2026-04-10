## ADDED Requirements

### Requirement: GET /health route
The API SHALL expose a `GET /health` endpoint that is publicly accessible without authentication. The endpoint MUST return `200 OK` when the service is healthy and `503 Service Unavailable` when degraded.

#### Scenario: Healthy response
- **WHEN** `GET /health` is called and the database is reachable
- **THEN** the response is `200 OK` with body `{ "status": "ok", "db": "ok" }`

#### Scenario: Degraded response when DB is unreachable
- **WHEN** `GET /health` is called and the database connection fails
- **THEN** the response is `503 Service Unavailable` with body `{ "status": "degraded", "db": "degraded" }`

### Requirement: Database liveness check
The `/health` endpoint SHALL verify database connectivity by executing a lightweight query (`SELECT 1`) against the primary DbContext. The check MUST complete within 2 seconds; a timeout MUST be treated as degraded.

#### Scenario: DB check times out
- **WHEN** the database does not respond within 2 seconds
- **THEN** the health endpoint returns `503` with `"db": "degraded"`

### Requirement: No authentication on /health
The `/health` route MUST be excluded from the `authInterceptor` and JWT validation middleware so that Railway and external monitors can poll it without credentials.

#### Scenario: Unauthenticated health check succeeds
- **WHEN** `GET /health` is called with no `Authorization` header
- **THEN** the endpoint responds normally (not `401 Unauthorized`)

### Requirement: /health registered in the API endpoint map
The health endpoint SHALL be registered in `src/BloomWatch.Api` using the same `Map*Endpoints()` pattern as other routes, under a `MapHealthEndpoints()` extension method.

#### Scenario: Endpoint is discoverable in Scalar docs
- **WHEN** the API starts and Scalar (`/scalar/v1`) is opened
- **THEN** `GET /health` appears in the API documentation
