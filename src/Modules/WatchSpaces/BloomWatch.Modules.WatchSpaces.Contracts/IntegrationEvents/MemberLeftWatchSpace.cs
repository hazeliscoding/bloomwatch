namespace BloomWatch.Modules.WatchSpaces.Contracts.IntegrationEvents;

public sealed record MemberLeftWatchSpace(
    Guid WatchSpaceId,
    Guid UserId,
    DateTime LeftAtUtc);
