using BloomWatch.SharedKernel.CQRS;

namespace BloomWatch.Modules.Analytics.Application.UseCases.GetRandomPick;

/// <summary>
/// Query to select a random anime from the watch space's backlog to help
/// the group decide what to watch next.
/// </summary>
/// <param name="WatchSpaceId">The unique identifier of the watch space to pick from.</param>
/// <param name="UserId">The requesting user's identifier (must be a member of the watch space).</param>
public sealed record GetRandomPickQuery(Guid WatchSpaceId, Guid UserId) : IQuery<RandomPickResult>;
