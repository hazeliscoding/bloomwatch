namespace BloomWatch.Modules.WatchSpaces.Contracts.IntegrationEvents;

/// <summary>
/// Integration event raised when a user becomes a member of a watch space,
/// either by accepting an invitation or being added as the founding owner.
/// </summary>
/// <remarks>
/// Consuming modules can use this event to synchronize membership-dependent data
/// (e.g., granting access to plants, creating activity feed entries).
/// Published via <c>IIntegrationEventPublisher</c> from the WatchSpaces application layer.
/// </remarks>
/// <param name="WatchSpaceId">The unique identifier of the watch space the user joined.</param>
/// <param name="UserId">The unique identifier of the user who joined.</param>
/// <param name="Role">The role assigned to the new member (e.g., "Owner", "Member").</param>
/// <param name="JoinedAtUtc">The UTC timestamp when the membership was created.</param>
public sealed record MemberJoinedWatchSpace(
    Guid WatchSpaceId,
    Guid UserId,
    string Role,
    DateTime JoinedAtUtc);
