## Context

Story 5.1 delivered `GET /watchspaces/{id}/dashboard`, which includes a compatibility score as one section of a larger composite response. The compatibility computation is currently implemented as `internal static` methods inside `GetDashboardSummaryQueryHandler`:

- `ComputeAnimeGaps` — filters anime with 2+ raters, computes per-anime pairwise gaps
- `ComputePairwiseGap` — mean of absolute differences for all rating pairs
- `ComputeCompatibility` — applies `max(0, round(100 - averageGap × 10))` formula, maps labels
- `GetCompatibilityLabel` — score-range-to-label mapping

Story 5.2 needs a lightweight dedicated endpoint returning only the compatibility score. The computation logic is identical to what the dashboard already uses.

## Goals / Non-Goals

**Goals:**
- Expose `GET /watchspaces/{id}/analytics/compatibility` returning only the compatibility payload
- Extract shared computation into a reusable helper so both endpoints stay in sync
- Keep the refactor internal — dashboard behaviour and response shape must not change

**Non-Goals:**
- Caching or rate-limiting (future concern)
- Changing the compatibility formula or labels
- Adding new computation logic beyond what Story 5.1 already implemented

## Decisions

### 1. Extract computation into `CompatibilityComputer` static helper

Move `ComputeAnimeGaps`, `ComputePairwiseGap`, `ComputeCompatibility`, and `GetCompatibilityLabel` from `GetDashboardSummaryQueryHandler` into a new `CompatibilityComputer` static class in the Application layer. Both the existing dashboard handler and the new compatibility handler call into this shared class.

**Why not a service/interface?** The methods are pure, stateless functions — no dependencies, no I/O. A static helper is the simplest and most testable approach. DI would add ceremony without benefit.

**Alternative considered:** Leave the methods in the dashboard handler and call them from the new handler. Rejected because it creates a conceptual dependency from one use-case to another, and the dashboard handler would need to expose internal methods publicly.

### 2. New use case: `GetCompatibilityQuery` + `GetCompatibilityQueryHandler`

A dedicated query handler that:
1. Checks membership via `IMembershipChecker`
2. Loads anime data via `IWatchSpaceAnalyticsDataSource`
3. Delegates computation to `CompatibilityComputer`
4. Returns a focused result DTO

**Why a separate handler instead of reusing the dashboard handler?** The dashboard handler computes much more (stats, currently-watching, backlog, rating-gap highlights). The compatibility endpoint should only pay for what it uses.

### 3. Reuse existing `CompatibilityResult` record, add a wrapper

Create `CompatibilityScoreResult(CompatibilityResult? Compatibility, string? Message)` as the endpoint response DTO. The inner `CompatibilityResult` record (already defined in `DashboardSummaryResult.cs`) is reused directly. The wrapper adds the nullable message field for the "Not enough data" case.

### 4. Add route to existing `AnalyticsEndpoints`

Add the new route to the existing endpoint class under a `/analytics/compatibility` path, keeping all analytics routes together.

## Risks / Trade-offs

- **[Risk] Refactoring dashboard handler could break existing tests** → Run all dashboard unit + integration tests after extraction to confirm no regressions. The static methods keep the same signatures.
- **[Risk] Two endpoints returning compatibility data could diverge** → Mitigated by sharing `CompatibilityComputer`. Both endpoints call the same code path.
