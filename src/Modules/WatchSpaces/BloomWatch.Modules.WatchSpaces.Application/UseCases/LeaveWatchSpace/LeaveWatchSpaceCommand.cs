namespace BloomWatch.Modules.WatchSpaces.Application.UseCases.LeaveWatchSpace;

public sealed record LeaveWatchSpaceCommand(Guid WatchSpaceId, Guid LeavingUserId);
