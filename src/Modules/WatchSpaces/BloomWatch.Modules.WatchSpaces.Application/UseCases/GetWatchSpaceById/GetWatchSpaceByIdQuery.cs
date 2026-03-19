namespace BloomWatch.Modules.WatchSpaces.Application.UseCases.GetWatchSpaceById;

/// <summary>
/// Query to retrieve detailed information about a specific watch space, including its members.
/// The requesting user must be a member of the watch space.
/// </summary>
/// <param name="WatchSpaceId">The unique identifier of the watch space to retrieve.</param>
/// <param name="RequestingUserId">The identifier of the user making the request (must be a member).</param>
public sealed record GetWatchSpaceByIdQuery(Guid WatchSpaceId, Guid RequestingUserId);

/// <summary>
/// A detailed projection of a watch space, including its full member list.
/// </summary>
/// <param name="WatchSpaceId">The unique identifier of the watch space.</param>
/// <param name="Name">The display name of the watch space.</param>
/// <param name="CreatedAt">The UTC timestamp when the watch space was created.</param>
/// <param name="Members">The list of members belonging to this watch space.</param>
public sealed record WatchSpaceDetail(
    Guid WatchSpaceId,
    string Name,
    DateTime CreatedAt,
    IReadOnlyList<MemberDetail> Members);

/// <summary>
/// A projection of a single member within a watch space.
/// </summary>
/// <param name="UserId">The unique identifier of the member.</param>
/// <param name="DisplayName">The member's display name.</param>
/// <param name="Role">The member's role in the watch space (e.g., <c>"Owner"</c> or <c>"Member"</c>).</param>
/// <param name="JoinedAt">The UTC timestamp when the member joined the watch space.</param>
public sealed record MemberDetail(Guid UserId, string DisplayName, string Role, DateTime JoinedAt);
