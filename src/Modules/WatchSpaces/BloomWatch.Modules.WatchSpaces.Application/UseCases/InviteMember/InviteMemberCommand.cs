namespace BloomWatch.Modules.WatchSpaces.Application.UseCases.InviteMember;

public sealed record InviteMemberCommand(Guid WatchSpaceId, string InvitedEmail, Guid RequestingUserId);

public sealed record InviteMemberResult(
    Guid InvitationId,
    string InvitedEmail,
    string Status,
    DateTime ExpiresAt,
    string Token);
