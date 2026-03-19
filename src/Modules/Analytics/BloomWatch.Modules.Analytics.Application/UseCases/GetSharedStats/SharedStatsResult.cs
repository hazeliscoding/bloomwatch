namespace BloomWatch.Modules.Analytics.Application.UseCases.GetSharedStats;

public sealed record SharedStatsResult(
    int TotalEpisodesWatchedTogether,
    int TotalFinished,
    int TotalDropped,
    int TotalWatchSessions,
    DateTime? MostRecentSessionDate);
