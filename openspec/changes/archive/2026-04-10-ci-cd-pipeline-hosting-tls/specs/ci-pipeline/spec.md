## ADDED Requirements

### Requirement: PR build check
The system SHALL run a full build of the .NET solution on every pull request targeting `main`. The build MUST use `dotnet build` with no warnings treated as errors.

#### Scenario: PR with clean build passes
- **WHEN** a pull request is opened or updated against `main`
- **THEN** the CI workflow runs `dotnet restore` and `dotnet build` and both exit with code 0

#### Scenario: PR with build error is blocked
- **WHEN** a pull request introduces a compilation error
- **THEN** the CI workflow exits with a non-zero code and the PR check is marked failed

### Requirement: Backend test gate
The CI workflow SHALL run all backend tests (`dotnet test`) on every PR. The test run MUST cover unit and integration test projects. Integration tests use SQLite in-memory and MUST NOT require a running database.

#### Scenario: All tests pass
- **WHEN** `dotnet test` runs against all test projects
- **THEN** every test passes and the check is marked green

#### Scenario: A failing test blocks merge
- **WHEN** one or more tests fail
- **THEN** the CI check is marked failed and the PR cannot be merged (branch protection required)

### Requirement: Frontend test gate
The CI workflow SHALL run frontend tests (`npm test`) on every PR targeting `main`. Tests run headless via Vitest.

#### Scenario: Frontend tests pass
- **WHEN** `npm ci` succeeds and `npm test` runs in the `src/BloomWatch.UI` directory
- **THEN** all Vitest tests pass and the check is marked green

#### Scenario: Frontend test failure blocks merge
- **WHEN** any Vitest test fails
- **THEN** the CI check exits non-zero and the PR is blocked

### Requirement: Workflow trigger scope
The CI workflow SHALL trigger on `pull_request` events targeting `main` only. It MUST NOT trigger on direct pushes to `main` (the deploy workflow handles those).

#### Scenario: PR opened triggers CI
- **WHEN** a pull request to `main` is opened or synchronized
- **THEN** the `ci.yml` workflow is triggered

#### Scenario: Push to non-main branch does not trigger CI
- **WHEN** a commit is pushed to a feature branch with no open PR
- **THEN** the `ci.yml` workflow does NOT run
