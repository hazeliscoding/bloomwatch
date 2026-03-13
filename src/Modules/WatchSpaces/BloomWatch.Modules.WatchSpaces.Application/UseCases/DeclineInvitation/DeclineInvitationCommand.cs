namespace BloomWatch.Modules.WatchSpaces.Application.UseCases.DeclineInvitation;

public sealed record DeclineInvitationCommand(string Token, string DecliningUserEmail);
