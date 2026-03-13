namespace BloomWatch.Modules.WatchSpaces.Application.UseCases.ListInvitations;

public sealed record ListInvitationsQuery(Guid WatchSpaceId, Guid RequestingUserId);

public sealed record InvitationDetail(
    Guid InvitationId,
    string InvitedEmail,
    string Status,
    DateTime ExpiresAt,
    DateTime CreatedAt);
