namespace BloomWatch.Modules.WatchSpaces.Application.UseCases.RenameWatchSpace;

public sealed record RenameWatchSpaceCommand(Guid WatchSpaceId, string NewName, Guid RequestingUserId);
