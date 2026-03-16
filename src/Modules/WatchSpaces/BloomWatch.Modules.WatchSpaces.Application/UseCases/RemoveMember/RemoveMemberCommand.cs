namespace BloomWatch.Modules.WatchSpaces.Application.UseCases.RemoveMember;

/// <summary>
/// Command to remove a member from a watch space. Only the owner can remove members.
/// </summary>
/// <param name="WatchSpaceId">The unique identifier of the watch space to remove the member from.</param>
/// <param name="TargetUserId">The identifier of the member to remove.</param>
/// <param name="RequestingUserId">The identifier of the user requesting the removal (must be the owner).</param>
public sealed record RemoveMemberCommand(Guid WatchSpaceId, Guid TargetUserId, Guid RequestingUserId);
