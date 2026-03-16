namespace BloomWatch.Modules.WatchSpaces.Application.UseCases.GetMyWatchSpaces;

/// <summary>
/// Query to retrieve all watch spaces that a user is a member of.
/// </summary>
/// <param name="UserId">The identifier of the user whose watch spaces to retrieve.</param>
public sealed record GetMyWatchSpacesQuery(Guid UserId);

/// <summary>
/// A summary projection of a watch space, including the requesting user's role within it.
/// </summary>
/// <param name="WatchSpaceId">The unique identifier of the watch space.</param>
/// <param name="Name">The display name of the watch space.</param>
/// <param name="CreatedAt">The UTC timestamp when the watch space was created.</param>
/// <param name="Role">The role of the requesting user in this watch space (e.g., <c>"Owner"</c> or <c>"Member"</c>).</param>
public sealed record WatchSpaceSummary(Guid WatchSpaceId, string Name, DateTime CreatedAt, string Role);
