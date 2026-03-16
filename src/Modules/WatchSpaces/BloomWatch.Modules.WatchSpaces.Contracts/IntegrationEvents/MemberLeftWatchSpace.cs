namespace BloomWatch.Modules.WatchSpaces.Contracts.IntegrationEvents;

/// <summary>
/// Integration event raised when a member leaves or is removed from a watch space.
/// </summary>
/// <remarks>
/// Consuming modules can use this event to revoke access, clean up membership-dependent
/// data, or update activity feeds. Published via <c>IIntegrationEventPublisher</c> from the
/// WatchSpaces application layer.
/// </remarks>
/// <param name="WatchSpaceId">The unique identifier of the watch space the user left.</param>
/// <param name="UserId">The unique identifier of the user who left or was removed.</param>
/// <param name="LeftAtUtc">The UTC timestamp when the membership was terminated.</param>
public sealed record MemberLeftWatchSpace(
    Guid WatchSpaceId,
    Guid UserId,
    DateTime LeftAtUtc);
