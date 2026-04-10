## ADDED Requirements

### Requirement: railway.toml project configuration
The repo SHALL contain a `railway.toml` at the repo root that declares the Railway project structure: two services (`api` and `frontend`) each pointing to their respective Dockerfiles.

#### Scenario: railway.toml is valid and parseable
- **WHEN** `railway up --dry-run` (or equivalent validation) is run
- **THEN** Railway accepts the `railway.toml` without errors

### Requirement: API service on Railway
The API SHALL be deployed as a Railway service using the repo root `Dockerfile`. The service MUST expose an HTTP port and have the Railway health check configured to poll `GET /health`.

#### Scenario: API service is reachable over HTTPS
- **WHEN** the service is deployed
- **THEN** `https://<railway-api-domain>/health` returns `200 OK`

#### Scenario: TLS is managed by Railway
- **WHEN** the service domain is accessed over HTTP
- **THEN** Railway redirects to HTTPS automatically (or serves only over HTTPS)

### Requirement: Frontend service on Railway
The Angular SPA SHALL be deployed as a separate Railway service using `src/BloomWatch.UI/Dockerfile`. The service MUST be publicly accessible over HTTPS.

#### Scenario: Frontend service is reachable over HTTPS
- **WHEN** the service is deployed
- **THEN** `https://<railway-frontend-domain>/` returns the Angular `index.html` with status `200`

### Requirement: Managed PostgreSQL on Railway
The production database SHALL be a Railway-managed PostgreSQL add-on within the same Railway project. The API service MUST connect to it using the private (internal) Railway network URL when both services are in the same environment.

#### Scenario: API connects to Railway PostgreSQL on startup
- **WHEN** the API container starts with `DATABASE_URL` pointing to the Railway internal PostgreSQL URL
- **THEN** the API starts successfully and EF Core migrations are already applied

#### Scenario: Database is not reachable without credentials
- **WHEN** a connection is attempted without the correct credentials
- **THEN** the connection is refused

### Requirement: Environment variables for all runtime config
All runtime configuration for the API (DB connection string, JWT key, email API key) SHALL be set as Railway environment variables. No secrets SHALL be hardcoded in `appsettings.json` or `railway.toml`.

#### Scenario: API reads config from environment
- **WHEN** the API starts and environment variables are set correctly
- **THEN** the application binds configuration from the environment (not from `appsettings.json` values)
