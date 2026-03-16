namespace BloomWatch.Modules.WatchSpaces.Domain.Enums;

/// <summary>
/// Represents the lifecycle states of an invitation to join a watch space.
/// An invitation begins as <see cref="Pending"/> and transitions exactly once
/// to one of the terminal states: <see cref="Accepted"/>, <see cref="Declined"/>,
/// or <see cref="Revoked"/>.
/// </summary>
public enum InvitationStatus
{
    /// <summary>
    /// The invitation has been sent but the invitee has not yet responded.
    /// This is the only state from which further transitions are allowed.
    /// </summary>
    Pending,

    /// <summary>
    /// The invitee accepted the invitation and became a member of the watch space.
    /// This is a terminal state.
    /// </summary>
    Accepted,

    /// <summary>
    /// The invitee explicitly declined the invitation.
    /// This is a terminal state.
    /// </summary>
    Declined,

    /// <summary>
    /// An owner of the watch space revoked the invitation before it was accepted or declined.
    /// This is a terminal state.
    /// </summary>
    Revoked
}
