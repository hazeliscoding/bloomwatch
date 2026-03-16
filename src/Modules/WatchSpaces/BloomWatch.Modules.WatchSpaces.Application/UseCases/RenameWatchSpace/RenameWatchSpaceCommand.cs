namespace BloomWatch.Modules.WatchSpaces.Application.UseCases.RenameWatchSpace;

/// <summary>
/// Command to rename an existing watch space. Only the owner can perform this operation.
/// </summary>
/// <param name="WatchSpaceId">The unique identifier of the watch space to rename.</param>
/// <param name="NewName">The new display name for the watch space.</param>
/// <param name="RequestingUserId">The identifier of the user requesting the rename.</param>
public sealed record RenameWatchSpaceCommand(Guid WatchSpaceId, string NewName, Guid RequestingUserId);
