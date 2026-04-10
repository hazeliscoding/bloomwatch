## 1. Health Endpoint (API layer)

- [x] 1.1 Add `MapHealthEndpoints()` static extension class in `src/BloomWatch.Api/Modules/Health/`
- [x] 1.2 Implement `GET /health` handler: run `SELECT 1` against the primary DbContext with a 2-second timeout; return `{ status, db }` with `200` or `503`
- [x] 1.3 Register `MapHealthEndpoints()` in `src/BloomWatch.Api/Program.cs` and exclude the route from JWT auth middleware
- [x] 1.4 Verify `/health` appears in Scalar docs at `/scalar/v1` and returns `200` with the local DB running

## 2. API Dockerfile

- [x] 2.1 Create `Dockerfile` at repo root: multi-stage (`sdk` → `aspnet` runtime), publish `src/BloomWatch.Api`, expose port `8080`
- [x] 2.2 Create `.dockerignore` at repo root excluding `bin/`, `obj/`, `.git/`, `tests/`, and `openspec/`
- [x] 2.3 Smoke-test locally: `docker build -t bloomwatch-api .` and `docker run -e DATABASE_URL=... bloomwatch-api` — confirm `/health` responds

## 3. Frontend Dockerfile

- [x] 3.1 Create `src/BloomWatch.UI/Dockerfile`: multi-stage (`node` build → `nginx:alpine` serve), run `npm ci && npm run build`, copy `dist/` to nginx html root
- [x] 3.2 Create `nginx.conf` in `src/BloomWatch.UI/`: rewrite unknown paths to `index.html`, set `Cache-Control: max-age=31536000, immutable` for hashed assets
- [x] 3.3 Create `.dockerignore` in `src/BloomWatch.UI/` excluding `node_modules/` and `dist/`
- [x] 3.4 Smoke-test locally: `docker build -f src/BloomWatch.UI/Dockerfile .` — confirm SPA serves and deep routes resolve

## 4. Environment Variable Configuration

- [x] 4.1 Update `appsettings.json` to replace any hard-coded connection strings and secrets with clearly fake development defaults (`Host=localhost;...`)
- [x] 4.2 Verify all secrets are wired via ASP.NET Core environment variable binding (`ConnectionStrings__DefaultConnection`, `JwtSettings__SecretKey`, `Email__ApiKey`)
- [x] 4.3 Create `docs/secrets.md` listing every required secret: name, purpose, Railway env var name, GitHub Actions secret name — no real values

## 5. Railway Infrastructure

- [x] 5.1 Create Railway project with two services: `bloomwatch-api` (Dockerfile at root) and `bloomwatch-frontend` (Dockerfile at `src/BloomWatch.UI/`)
- [x] 5.2 Provision Railway PostgreSQL add-on; confirm the API service can reach it via the internal private URL
- [x] 5.3 Set Railway environment variables for the API service: `DATABASE_URL`, `JWT_SECRET_KEY`, `EMAIL_API_KEY`
- [x] 5.4 Create `railway.toml` at repo root declaring both services and their Dockerfile paths
- [ ] 5.5 Confirm both services deploy and are reachable over HTTPS on their Railway subdomains

## 6. CI Workflow (GitHub Actions)

- [x] 6.1 Create `.github/workflows/ci.yml` triggered on `pull_request` targeting `main`
- [x] 6.2 Add backend job: `dotnet restore` → `dotnet build` → `dotnet test` (all test projects)
- [x] 6.3 Add frontend job: `npm ci` → `npm test` (runs in `src/BloomWatch.UI/`)
- [ ] 6.4 Enable branch protection on `main` requiring the `ci` checks to pass before merge
- [ ] 6.5 Verify on a test branch: open a PR, confirm both jobs run green

## 7. Deploy Workflow (GitHub Actions)

- [x] 7.1 Create `.github/workflows/deploy.yml` triggered on `push` to `main`
- [x] 7.2 Add image build + push job: build API and frontend Docker images, tag with `latest` and short commit SHA, push to `ghcr.io/hazeliscoding/bloomwatch-{api,frontend}`
- [x] 7.3 Add migration job (runs after image push): run `dotnet ef database update` per module in order — Identity → WatchSpaces → AniListSync → AnimeTracking → Analytics — using `DATABASE_URL` from GitHub Actions secret
- [x] 7.4 Add deploy job (runs after migration job): trigger Railway redeploy for both services via `railway up` or Railway webhook; set `RAILWAY_TOKEN` as a GitHub Actions secret
- [x] 7.5 Set GitHub Actions repository secrets: `RAILWAY_TOKEN`, `GHCR_TOKEN`, `DATABASE_URL` (production)
- [ ] 7.6 Merge a test commit to `main`; confirm the full deploy pipeline runs end-to-end and `/health` returns `200` on the Railway domain
