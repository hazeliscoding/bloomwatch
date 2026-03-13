namespace BloomWatch.Modules.WatchSpaces.Application.UseCases.CreateWatchSpace;

public sealed record CreateWatchSpaceCommand(string Name, Guid RequestingUserId);

public sealed record CreateWatchSpaceResult(Guid WatchSpaceId, string Name, DateTime CreatedAt, string Role);
