using BloomWatch.Modules.Analytics.Application.UseCases.GetDashboardSummary;

namespace BloomWatch.Modules.Analytics.Application.UseCases.GetCompatibility;

/// <summary>
/// Result for the standalone compatibility endpoint, wrapping the shared <see cref="CompatibilityResult"/>
/// with an explanatory message when the score cannot be computed.
/// </summary>
/// <param name="Compatibility">The computed compatibility result, or <c>null</c> if there are fewer than 2 members with shared rated anime.</param>
/// <param name="Message">A human-readable explanation when <paramref name="Compatibility"/> is <c>null</c> (e.g., "Need at least 2 members with shared ratings").</param>
public sealed record CompatibilityScoreResult(
    CompatibilityResult? Compatibility,
    string? Message);
