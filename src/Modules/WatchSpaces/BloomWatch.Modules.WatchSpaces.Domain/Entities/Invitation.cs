using BloomWatch.Modules.WatchSpaces.Domain.Enums;
using BloomWatch.Modules.WatchSpaces.Domain.ValueObjects;

namespace BloomWatch.Modules.WatchSpaces.Domain.Entities;

/// <summary>
/// Represents an invitation for a user to join a <see cref="Aggregates.WatchSpace"/>.
/// <para>
/// Invitations follow a linear lifecycle: they are created in
/// <see cref="InvitationStatus.Pending"/> status and transition exactly once to a terminal
/// state (<see cref="InvitationStatus.Accepted"/>, <see cref="InvitationStatus.Declined"/>,
/// or <see cref="InvitationStatus.Revoked"/>). Each invitation carries a unique token used
/// for acceptance or decline by the invitee.
/// </para>
/// <para>
/// This entity is owned by the <see cref="Aggregates.WatchSpace"/> aggregate root and must
/// not be created or mutated outside of it.
/// </para>
/// </summary>
public sealed class Invitation
{
    /// <summary>
    /// Gets the unique identifier for this invitation.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the identifier of the watch space this invitation belongs to.
    /// </summary>
    public WatchSpaceId WatchSpaceId { get; private set; }

    /// <summary>
    /// Gets the identifier of the user (owner) who created this invitation.
    /// </summary>
    public Guid InvitedByUserId { get; private set; }

    /// <summary>
    /// Gets the email address of the person being invited.
    /// Email comparison is case-insensitive throughout the domain.
    /// </summary>
    public string InvitedEmail { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the unique, opaque token used by the invitee to accept or decline
    /// this invitation. Generated as a 32-character hex string (GUID without hyphens).
    /// </summary>
    public string Token { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the current status of this invitation.
    /// See <see cref="InvitationStatus"/> for the full set of lifecycle states.
    /// </summary>
    public InvitationStatus Status { get; private set; }

    /// <summary>
    /// Gets the UTC timestamp after which this invitation is no longer valid.
    /// An expired pending invitation cannot be accepted.
    /// </summary>
    public DateTime ExpiresAtUtc { get; private set; }

    /// <summary>
    /// Gets the UTC timestamp of when this invitation was created.
    /// </summary>
    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>
    /// Gets the UTC timestamp of when this invitation was accepted,
    /// or <see langword="null"/> if it has not been accepted.
    /// </summary>
    public DateTime? AcceptedAtUtc { get; private set; }

    // Required by EF Core
    private Invitation() { }

    /// <summary>
    /// Initializes a new <see cref="Invitation"/> in <see cref="InvitationStatus.Pending"/> status
    /// with an auto-generated identifier and token. This constructor is internal because
    /// invitations must be created through the <see cref="Aggregates.WatchSpace"/> aggregate root.
    /// </summary>
    /// <param name="watchSpaceId">The identifier of the watch space the invitee will join.</param>
    /// <param name="invitedByUserId">The identifier of the owner who is sending the invitation.</param>
    /// <param name="invitedEmail">The email address of the person being invited.</param>
    /// <param name="expiresAtUtc">The UTC timestamp after which this invitation expires.</param>
    /// <param name="createdAtUtc">The UTC timestamp of when the invitation is created.</param>
    internal Invitation(
        WatchSpaceId watchSpaceId,
        Guid invitedByUserId,
        string invitedEmail,
        DateTime expiresAtUtc,
        DateTime createdAtUtc)
    {
        Id = Guid.NewGuid();
        WatchSpaceId = watchSpaceId;
        InvitedByUserId = invitedByUserId;
        InvitedEmail = invitedEmail;
        Token = Guid.NewGuid().ToString("N");
        Status = InvitationStatus.Pending;
        ExpiresAtUtc = expiresAtUtc;
        CreatedAtUtc = createdAtUtc;
    }

    /// <summary>
    /// Transitions the invitation to <see cref="InvitationStatus.Accepted"/> status
    /// and records the acceptance timestamp.
    /// </summary>
    /// <param name="acceptedAtUtc">The UTC timestamp of when the invitee accepted.</param>
    internal void Accept(DateTime acceptedAtUtc)
    {
        Status = InvitationStatus.Accepted;
        AcceptedAtUtc = acceptedAtUtc;
    }

    /// <summary>
    /// Transitions the invitation to <see cref="InvitationStatus.Declined"/> status.
    /// Called when the invitee explicitly declines.
    /// </summary>
    internal void Decline() => Status = InvitationStatus.Declined;

    /// <summary>
    /// Transitions the invitation to <see cref="InvitationStatus.Revoked"/> status.
    /// Called when an owner revokes a pending invitation before the invitee responds.
    /// </summary>
    internal void Revoke() => Status = InvitationStatus.Revoked;

    /// <summary>
    /// Determines whether this invitation has expired as of the given point in time.
    /// </summary>
    /// <param name="now">The current UTC timestamp to compare against <see cref="ExpiresAtUtc"/>.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="now"/> is past the expiration time;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool IsExpired(DateTime now) => ExpiresAtUtc < now;
}
