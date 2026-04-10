## ADDED Requirements

### Requirement: Deploy on push to main
The system SHALL automatically deploy to Railway when a commit is pushed (or a PR is merged) to `main`. The deploy workflow MUST be a separate GitHub Actions workflow file from the CI workflow.

#### Scenario: Merge to main triggers deploy
- **WHEN** a pull request is merged into `main`
- **THEN** the `deploy.yml` workflow runs automatically

#### Scenario: Direct push to main triggers deploy
- **WHEN** a commit is pushed directly to `main`
- **THEN** the `deploy.yml` workflow runs automatically

### Requirement: Docker image build and push
The deploy workflow SHALL build Docker images for the API and frontend and push them to GitHub Container Registry (ghcr.io) before triggering the Railway deploy.

#### Scenario: Images tagged with commit SHA
- **WHEN** the deploy workflow builds Docker images
- **THEN** images are tagged with both `latest` and the short commit SHA (e.g., `ghcr.io/hazeliscoding/bloomwatch-api:abc1234`)

#### Scenario: Push fails if build fails
- **WHEN** either Docker image fails to build
- **THEN** the workflow exits with a non-zero code and no deploy is triggered

### Requirement: Database migrations before deploy
The deploy workflow SHALL run EF Core migrations for all modules against the production PostgreSQL instance before the new Docker image goes live. Migrations MUST run in module dependency order: Identity → WatchSpaces → AniListSync → AnimeTracking → Analytics.

#### Scenario: Successful migration proceeds to deploy
- **WHEN** all migration commands exit with code 0
- **THEN** the workflow proceeds to trigger the Railway service deploy

#### Scenario: Migration failure aborts deploy
- **WHEN** any migration command exits with a non-zero code
- **THEN** the workflow stops immediately, the Railway deploy is NOT triggered, and the currently running image stays live

### Requirement: Railway service deployment
The deploy workflow SHALL trigger a redeploy of both Railway services (API and frontend) after images are pushed and migrations succeed.

#### Scenario: Both services redeployed
- **WHEN** migration step succeeds and new images are in the registry
- **THEN** both Railway services pull the new images and restart

### Requirement: Secrets available to deploy workflow
All secrets needed by the deploy workflow (Railway token, GHCR credentials, DB connection string) SHALL be stored as GitHub Actions repository secrets and MUST NOT appear in workflow YAML files.

#### Scenario: Secrets injected at runtime
- **WHEN** the deploy workflow runs
- **THEN** secrets are referenced via `${{ secrets.SECRET_NAME }}` and never echoed to logs
