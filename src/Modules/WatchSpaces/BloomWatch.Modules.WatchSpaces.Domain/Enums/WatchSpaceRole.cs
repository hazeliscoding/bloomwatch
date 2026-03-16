namespace BloomWatch.Modules.WatchSpaces.Domain.Enums;

/// <summary>
/// Defines the roles a user can hold within a watch space.
/// Roles determine the set of operations a member is authorized to perform.
/// </summary>
public enum WatchSpaceRole
{
    /// <summary>
    /// Full administrative control over the watch space. Owners can rename the space,
    /// invite and remove members, revoke invitations, and transfer ownership.
    /// Every watch space must have at least one owner at all times.
    /// </summary>
    Owner,

    /// <summary>
    /// Standard participant in the watch space. Members can view content and leave
    /// the space voluntarily, but cannot perform administrative operations.
    /// </summary>
    Member
}
