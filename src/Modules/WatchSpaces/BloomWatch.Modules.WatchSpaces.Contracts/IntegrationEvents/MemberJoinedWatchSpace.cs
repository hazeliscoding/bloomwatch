namespace BloomWatch.Modules.WatchSpaces.Contracts.IntegrationEvents;

public sealed record MemberJoinedWatchSpace(
    Guid WatchSpaceId,
    Guid UserId,
    string Role,
    DateTime JoinedAtUtc);
