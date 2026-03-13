namespace BloomWatch.Modules.WatchSpaces.Application.UseCases.AcceptInvitation;

public sealed record AcceptInvitationCommand(string Token, Guid AcceptingUserId, string AcceptingUserEmail);
