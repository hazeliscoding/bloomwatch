namespace BloomWatch.Modules.WatchSpaces.Application.UseCases.RevokeInvitation;

public sealed record RevokeInvitationCommand(Guid WatchSpaceId, Guid InvitationId, Guid RequestingUserId);
