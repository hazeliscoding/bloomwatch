using BloomWatch.Modules.Analytics.Application.UseCases.GetDashboardSummary;

namespace BloomWatch.Modules.Analytics.Application.UseCases.GetCompatibility;

public sealed record CompatibilityScoreResult(
    CompatibilityResult? Compatibility,
    string? Message);
