using BloomWatch.SharedKernel.CQRS;

namespace BloomWatch.Modules.Analytics.Application.UseCases.GetSharedStats;

/// <summary>
/// Query to retrieve aggregate statistics about a watch space's shared viewing activity,
/// including total episodes watched together, finished shows, and dropped shows.
/// </summary>
/// <param name="WatchSpaceId">The unique identifier of the watch space to query.</param>
/// <param name="UserId">The requesting user's identifier (must be a member of the watch space).</param>
public sealed record GetSharedStatsQuery(Guid WatchSpaceId, Guid UserId) : IQuery<SharedStatsResult>;
