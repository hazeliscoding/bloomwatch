## ADDED Requirements

### Requirement: API Dockerfile — multi-stage build
The API SHALL have a multi-stage Dockerfile at the repo root. Stage 1 (`sdk`) restores and publishes the .NET application. Stage 2 (`runtime`) uses the `mcr.microsoft.com/dotnet/aspnet` image and contains only the published output. The final image MUST NOT contain the .NET SDK.

#### Scenario: Image builds without error
- **WHEN** `docker build -f Dockerfile .` is run from the repo root
- **THEN** the build completes successfully with a runnable image

#### Scenario: Container starts and listens
- **WHEN** the API container starts with required environment variables set
- **THEN** the process binds to `$PORT` (or `8080` by default) and responds to HTTP requests

#### Scenario: SDK not present in final image
- **WHEN** the final API image is inspected
- **THEN** `dotnet sdk` tooling is absent (only the ASP.NET runtime layer is present)

### Requirement: Frontend Dockerfile — nginx SPA server
The Angular frontend SHALL have a multi-stage Dockerfile at `src/BloomWatch.UI/Dockerfile`. Stage 1 (`build`) runs `npm ci` and `npm run build`. Stage 2 (`serve`) uses `nginx:alpine` to serve the compiled output. A custom `nginx.conf` MUST be included that rewrites all 404s to `index.html` for Angular's client-side router.

#### Scenario: Frontend image builds without error
- **WHEN** `docker build -f src/BloomWatch.UI/Dockerfile .` is run from the repo root
- **THEN** the build completes successfully

#### Scenario: SPA routing handled by nginx
- **WHEN** a request is made for a deep Angular route (e.g., `/watch-spaces/123`)
- **THEN** nginx serves `index.html` rather than returning 404

#### Scenario: Hashed assets cached correctly
- **WHEN** a request is made for a hashed JS or CSS asset (e.g., `main.abc123.js`)
- **THEN** nginx returns `Cache-Control: max-age=31536000, immutable`

### Requirement: .dockerignore files
Both Dockerfile contexts SHALL have a `.dockerignore` file that excludes `node_modules`, `dist`, `bin`, `obj`, `.git`, and other artifacts that should not be copied into the build context.

#### Scenario: Build context excludes generated artifacts
- **WHEN** a Docker build is triggered
- **THEN** `node_modules`, `bin/`, and `obj/` directories are not sent to the Docker daemon
