namespace BloomWatch.Modules.WatchSpaces.Application.UseCases.RemoveMember;

public sealed record RemoveMemberCommand(Guid WatchSpaceId, Guid TargetUserId, Guid RequestingUserId);
