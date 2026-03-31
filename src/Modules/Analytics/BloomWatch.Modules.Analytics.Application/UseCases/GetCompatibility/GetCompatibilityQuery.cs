using BloomWatch.SharedKernel.CQRS;

namespace BloomWatch.Modules.Analytics.Application.UseCases.GetCompatibility;

/// <summary>
/// Query to compute the taste-compatibility score for a watch space based on
/// members' overlapping anime ratings.
/// </summary>
/// <param name="WatchSpaceId">The unique identifier of the watch space to analyze.</param>
/// <param name="UserId">The requesting user's identifier (must be a member of the watch space).</param>
public sealed record GetCompatibilityQuery(Guid WatchSpaceId, Guid UserId) : IQuery<CompatibilityScoreResult>;
