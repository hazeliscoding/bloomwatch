namespace BloomWatch.Modules.Analytics.Application.UseCases.GetSharedStats;

public sealed record GetSharedStatsQuery(Guid WatchSpaceId, Guid UserId);
