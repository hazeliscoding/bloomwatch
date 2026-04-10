## Why

BloomWatch has no automated build, test, or deployment pipeline — every release is a manual process with no staging environment, no TLS, and no secrets management. Closing #5 establishes a production-grade CI/CD foundation required before v0.3.0 can ship.

## What Changes

- Add GitHub Actions workflows: PR checks (build + test) and a deploy pipeline (staging → production)
- Dockerize the .NET 10 API and the Angular 21 frontend
- Host both containers on Railway (separate services)
- Provision a production PostgreSQL instance on Railway; run EF Core migrations automatically on deploy
- Manage all secrets (JWT key, DB connection string, email API key) via Railway environment variables — never committed to source
- Expose a `/health` endpoint on the API for uptime monitoring
- TLS handled by Railway's managed certificates

## Capabilities

### New Capabilities

- `ci-pipeline`: GitHub Actions workflow that builds, tests, and lints on every PR — no merge without green checks
- `cd-pipeline`: GitHub Actions workflow that builds Docker images, pushes to a registry, and deploys to Railway on push to `main`
- `docker-packaging`: Dockerfiles for the .NET API and the Angular frontend (nginx-served static build)
- `railway-infrastructure`: Railway project config (`railway.toml`) for API service, frontend service, and managed PostgreSQL
- `health-endpoint`: `/health` route on the API returning service + DB liveness status
- `secrets-management`: Convention and documentation for all runtime secrets via Railway environment variables

### Modified Capabilities

_(none — no existing spec-level behavior changes)_

## Impact

- **New files**: `.github/workflows/ci.yml`, `.github/workflows/deploy.yml`, `Dockerfile` (API), `src/BloomWatch.UI/Dockerfile`, `railway.toml`, `nginx.conf`
- **API**: add `/health` endpoint in `src/BloomWatch.Api`
- **`appsettings.json`**: connection strings and secrets replaced by environment variable references; no sensitive values in source
- **EF Core migrations**: deploy workflow runs `dotnet ef database update` (or equivalent migration runner) against the Railway PostgreSQL instance before the new image goes live
- **Dependencies**: Docker, GitHub Actions, Railway CLI
