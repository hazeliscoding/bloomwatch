namespace BloomWatch.Modules.WatchSpaces.Application.UseCases.GetMyWatchSpaces;

public sealed record GetMyWatchSpacesQuery(Guid UserId);

public sealed record WatchSpaceSummary(Guid WatchSpaceId, string Name, DateTime CreatedAt, string Role);
