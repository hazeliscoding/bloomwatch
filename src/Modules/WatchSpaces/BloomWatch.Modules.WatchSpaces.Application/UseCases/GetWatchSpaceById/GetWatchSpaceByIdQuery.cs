namespace BloomWatch.Modules.WatchSpaces.Application.UseCases.GetWatchSpaceById;

public sealed record GetWatchSpaceByIdQuery(Guid WatchSpaceId, Guid RequestingUserId);

public sealed record WatchSpaceDetail(
    Guid WatchSpaceId,
    string Name,
    DateTime CreatedAt,
    IReadOnlyList<MemberDetail> Members);

public sealed record MemberDetail(Guid UserId, string Role, DateTime JoinedAt);
