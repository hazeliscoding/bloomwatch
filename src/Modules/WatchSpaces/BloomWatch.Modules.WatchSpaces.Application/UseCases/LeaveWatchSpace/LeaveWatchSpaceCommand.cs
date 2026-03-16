namespace BloomWatch.Modules.WatchSpaces.Application.UseCases.LeaveWatchSpace;

/// <summary>
/// Command for a member to voluntarily leave a watch space. Owners cannot leave;
/// they must transfer ownership first.
/// </summary>
/// <param name="WatchSpaceId">The unique identifier of the watch space to leave.</param>
/// <param name="LeavingUserId">The identifier of the member who is leaving.</param>
public sealed record LeaveWatchSpaceCommand(Guid WatchSpaceId, Guid LeavingUserId);
