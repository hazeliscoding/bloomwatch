## ADDED Requirements

### Requirement: No secrets in source control
The repository SHALL NOT contain any production secrets (database connection strings, JWT signing keys, email API keys, Railway tokens, or registry credentials) in any committed file, including `appsettings.json`, `appsettings.Production.json`, `railway.toml`, or GitHub Actions workflow YAML.

#### Scenario: appsettings.json contains no real values
- **WHEN** `appsettings.json` is inspected in the repository
- **THEN** sensitive fields reference environment variable placeholders or contain clearly fake development defaults only

#### Scenario: Workflow YAML contains no inline secrets
- **WHEN** `.github/workflows/*.yml` files are inspected
- **THEN** secrets are referenced exclusively via `${{ secrets.SECRET_NAME }}` syntax with no literal values

### Requirement: ASP.NET Core environment variable binding
The API SHALL read all production secrets from environment variables using ASP.NET Core's standard configuration pipeline. Environment variable names MUST map to the corresponding `IConfiguration` key using the `__` double-underscore separator convention.

#### Scenario: Configuration bound from environment
- **WHEN** the API starts in a Railway environment with `ConnectionStrings__DefaultConnection` set
- **THEN** `IConfiguration["ConnectionStrings:DefaultConnection"]` returns the Railway-provided value

### Requirement: GitHub Actions secrets for deploy credentials
Railway deploy credentials (token) and GHCR write credentials SHALL be stored as GitHub Actions repository secrets. They MUST be named consistently: `RAILWAY_TOKEN` and `GHCR_TOKEN`.

#### Scenario: Deploy workflow uses named secrets
- **WHEN** the `deploy.yml` workflow runs
- **THEN** it authenticates to Railway and GHCR using `${{ secrets.RAILWAY_TOKEN }}` and `${{ secrets.GHCR_TOKEN }}` respectively

### Requirement: Secrets documentation
A `docs/secrets.md` file SHALL document every required secret, its purpose, where it is stored, and the environment variable name used at runtime. This file MUST NOT contain any actual secret values.

#### Scenario: Secrets doc is complete
- **WHEN** `docs/secrets.md` is read
- **THEN** every secret used by the application and deploy pipeline is listed with its name, purpose, and storage location
