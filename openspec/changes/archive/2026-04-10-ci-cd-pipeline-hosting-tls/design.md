## Context

BloomWatch is a DDD modular monolith: a single .NET 10 API process backed by PostgreSQL, plus an Angular 21 SPA. Currently there is no automated pipeline — builds, tests, and deployments are all manual. Railway is the chosen hosting provider for both the API and the frontend (confirmed). The goal is a minimal, maintainable CI/CD foundation that unblocks v0.3.0 and beyond.

## Goals / Non-Goals

**Goals:**
- Automated PR checks (build + unit + integration tests) before any merge to `main`
- Automated deploy to Railway on push to `main`
- Docker-based packaging for both API and frontend
- Production PostgreSQL on Railway with migrations run at deploy time
- TLS via Railway managed certificates (zero-config)
- All secrets in Railway environment variables — nothing sensitive in source
- `/health` endpoint on the API for basic uptime monitoring

**Non-Goals:**
- Separate staging environment (deferred to v0.4+)
- Blue/green or canary deploys
- Custom domain or DNS management (Railway subdomain is sufficient for v0.3.0)
- Kubernetes or container orchestration beyond Railway's built-in scheduler
- Frontend E2E tests in CI (Vitest unit tests only for now)

## Decisions

### D1: Railway for both API and frontend

Railway supports Dockerfile-based deployments, managed PostgreSQL, and environment variable secrets out of the box. Alternatives (Fly.io, Azure App Service, Render) were considered — Railway wins on simplicity for a pre-1.0 project with a single developer. Frontend is served as a static nginx build via a separate Railway service rather than coupling it to the API container.

### D2: Two separate Dockerfiles, not a monorepo multi-stage build

The API (`Dockerfile` at repo root) and the frontend (`src/BloomWatch.UI/Dockerfile`) are built and deployed independently. This keeps build times short, allows independent redeploys, and avoids a fat combined image. Railway's service-per-Dockerfile model maps naturally to this.

**API Dockerfile strategy**: multi-stage — `sdk` image to restore/build/publish, then `aspnet` runtime image. The published output is copied into the final stage; no SDK in the image.

**Frontend Dockerfile strategy**: multi-stage — `node` image to `npm ci` + `npm run build`, then `nginx:alpine` to serve `dist/`. A custom `nginx.conf` handles SPA routing (all 404s rewrite to `index.html`) and sets cache headers for hashed assets.

### D3: GitHub Actions for CI/CD (not Railway's built-in Git deploy)

Railway's native Git integration is convenient but offers no control over the test gate. GitHub Actions gives us a proper PR check workflow that blocks merge on failure, then a separate deploy workflow on push to `main`. Secrets (Railway token, registry credentials) live in GitHub Actions secrets.

**CI workflow** (`ci.yml`): triggered on every PR to `main`.
1. `dotnet restore` + `dotnet build`
2. `dotnet test` (unit + integration — SQLite in-memory, no external DB needed)
3. `npm ci` + `npm test` (Vitest, headless)

**Deploy workflow** (`deploy.yml`): triggered on push to `main`.
1. Build and push Docker images to GitHub Container Registry (ghcr.io)
2. Run EF Core migrations via `dotnet ef database update` against the Railway PostgreSQL connection string (passed as a secret)
3. Trigger Railway deploy via `railway up` or Railway webhook

### D4: EF Core migrations run as a deploy step, not in app startup

Running `Database.MigrateAsync()` on startup is fragile under concurrent instances and hides migration failures. Instead, the deploy workflow runs migrations as an explicit step before the new image goes live. If migrations fail, the deploy is aborted and the old image stays running.

Migration runner: a standalone `dotnet ef database update` call using the Infrastructure project as the startup assembly, invoked per-module in dependency order (Identity → WatchSpaces → AniListSync → AnimeTracking → Analytics).

### D5: `/health` endpoint — shallow check

A single `GET /health` route returning `200 OK` with a JSON body: `{ "status": "ok", "db": "ok"|"degraded" }`. The DB check opens a connection and runs `SELECT 1`. No deep business-logic checks. Railway can poll this for restart-on-failure behavior.

### D6: Secrets convention

| Secret | Stored in | Variable name |
|---|---|---|
| DB connection string | Railway env | `DATABASE_URL` |
| JWT signing key | Railway env | `JWT_SECRET_KEY` |
| Email API key | Railway env | `EMAIL_API_KEY` |
| Railway deploy token | GitHub Actions secret | `RAILWAY_TOKEN` |
| GHCR write token | GitHub Actions secret | `GHCR_TOKEN` |

`appsettings.json` uses `Environment.GetEnvironmentVariable` bindings via the standard ASP.NET Core configuration pipeline — no hard-coded values.

## Risks / Trade-offs

- **Single Railway environment (no staging)** → Integration issues only surface in production. Mitigation: the CI test suite (SQLite in-memory) must cover the critical paths; staging can be added in v0.4+.
- **Migration step in deploy workflow requires DB connectivity from CI runner** → The Railway PostgreSQL instance must be reachable from GitHub Actions. Mitigation: Railway's public networking is on by default; use the public connection string with SSL required.
- **ghcr.io image pull during Railway deploy adds latency** → Acceptable for pre-1.0. Railway caches layers between deploys.
- **`railway up` CLI is not officially stable** → Pin the Railway CLI version in the workflow. If the CLI breaks, fall back to the Railway webhook deploy trigger (also supported).
- **EF Core multi-module migration order** → Migrations must run in dependency order. The deploy script documents and enforces this order explicitly; a wrong order will fail fast with a foreign-key error rather than silently corrupt data.

## Migration Plan

1. Add `/health` endpoint to the API (no DB changes needed)
2. Write and test Dockerfiles locally (`docker build` + `docker run`)
3. Set up Railway project: API service, frontend service, PostgreSQL add-on
4. Add Railway and GHCR secrets to GitHub repository settings
5. Commit `railway.toml`, `nginx.conf`, both Dockerfiles
6. Add `ci.yml` — verify PR checks pass on a test branch
7. Add `deploy.yml` — trigger first deploy manually, verify HTTPS access and `/health`
8. Tag v0.3.0

**Rollback**: Railway keeps the previous successful deploy and supports one-click rollback from the dashboard. The migration step is the only irreversible part — always write forward-compatible migrations (additive only, no destructive column drops without a two-phase deploy).

## Open Questions

- Should the Railway PostgreSQL instance use the internal (private network) URL for the API connection, or the public URL? → Prefer internal URL (lower latency, no egress cost); requires both services to be in the same Railway project/environment.
- Do we need `HEALTHCHECK` in the Dockerfile, or is the Railway health poll sufficient? → Railway's HTTP health check on `/health` is sufficient; `HEALTHCHECK` is optional.
