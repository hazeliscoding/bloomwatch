namespace BloomWatch.Modules.WatchSpaces.Application.UseCases.CreateWatchSpace;

/// <summary>
/// Command to create a new watch space. The requesting user becomes the owner.
/// </summary>
/// <param name="Name">The display name for the new watch space.</param>
/// <param name="RequestingUserId">The identifier of the user creating the watch space.</param>
public sealed record CreateWatchSpaceCommand(string Name, Guid RequestingUserId);

/// <summary>
/// The result returned after a watch space is successfully created.
/// </summary>
/// <param name="WatchSpaceId">The unique identifier assigned to the newly created watch space.</param>
/// <param name="Name">The display name of the watch space.</param>
/// <param name="CreatedAt">The UTC timestamp when the watch space was created.</param>
/// <param name="Role">The role assigned to the creating user (always <c>"Owner"</c>).</param>
public sealed record CreateWatchSpaceResult(Guid WatchSpaceId, string Name, DateTime CreatedAt, string Role);
