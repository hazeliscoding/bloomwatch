namespace BloomWatch.Modules.Analytics.Application.UseCases.GetSharedStats;

/// <summary>
/// Aggregate statistics about a watch space's shared viewing activity.
/// </summary>
/// <param name="TotalEpisodesWatchedTogether">Sum of shared episodes watched across all tracked anime.</param>
/// <param name="TotalFinished">Number of anime the group has finished watching together.</param>
/// <param name="TotalDropped">Number of anime the group has dropped.</param>
public sealed record SharedStatsResult(
    int TotalEpisodesWatchedTogether,
    int TotalFinished,
    int TotalDropped);
