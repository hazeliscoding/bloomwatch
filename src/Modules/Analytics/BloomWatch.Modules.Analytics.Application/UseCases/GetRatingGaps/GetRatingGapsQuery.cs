using BloomWatch.SharedKernel.CQRS;

namespace BloomWatch.Modules.Analytics.Application.UseCases.GetRatingGaps;

/// <summary>
/// Query to retrieve all anime in a watch space where members have divergent ratings,
/// sorted by descending gap magnitude with alphabetical title tie-breaking.
/// </summary>
/// <param name="WatchSpaceId">The unique identifier of the watch space to analyze.</param>
/// <param name="UserId">The requesting user's identifier (must be a member of the watch space).</param>
public sealed record GetRatingGapsQuery(Guid WatchSpaceId, Guid UserId) : IQuery<RatingGapsResult>;
