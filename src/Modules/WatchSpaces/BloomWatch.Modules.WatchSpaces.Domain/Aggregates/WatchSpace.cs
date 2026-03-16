using BloomWatch.Modules.WatchSpaces.Domain.Entities;
using BloomWatch.Modules.WatchSpaces.Domain.Enums;
using BloomWatch.Modules.WatchSpaces.Domain.Exceptions;
using BloomWatch.Modules.WatchSpaces.Domain.ValueObjects;

namespace BloomWatch.Modules.WatchSpaces.Domain.Aggregates;

/// <summary>
/// Aggregate root representing a collaborative watch space where members track plants together.
/// <para>
/// A watch space enforces the following invariants:
/// <list type="bullet">
///   <item><description>The name must be non-empty and at most 100 characters.</description></item>
///   <item><description>Membership is capped at 20 members.</description></item>
///   <item><description>At least one <see cref="WatchSpaceRole.Owner"/> must exist at all times.</description></item>
///   <item><description>Only owners may perform administrative operations (rename, invite, remove, revoke, transfer).</description></item>
///   <item><description>Duplicate pending invitations for the same email address are not allowed.</description></item>
/// </list>
/// </para>
/// <para>
/// All mutations to members and invitations must go through this aggregate root to preserve
/// consistency boundaries.
/// </para>
/// </summary>
public sealed class WatchSpace
{
    private const int MaxNameLength = 100;
    private const int MaxMembers = 20;

    private readonly List<WatchSpaceMember> _members = [];
    private readonly List<Invitation> _invitations = [];

    /// <summary>
    /// Gets the strongly-typed unique identifier for this watch space.
    /// </summary>
    public WatchSpaceId Id { get; private set; }

    /// <summary>
    /// Gets the display name of this watch space. Must be non-empty and at most 100 characters.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the identifier of the user who originally created this watch space.
    /// </summary>
    public Guid CreatedByUserId { get; private set; }

    /// <summary>
    /// Gets the UTC timestamp of when this watch space was created.
    /// </summary>
    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>
    /// Gets the UTC timestamp of the most recent modification to this watch space
    /// or any of its child entities (members, invitations).
    /// </summary>
    public DateTime UpdatedAtUtc { get; private set; }

    /// <summary>
    /// Gets the current members of this watch space as a read-only list.
    /// </summary>
    public IReadOnlyList<WatchSpaceMember> Members => _members.AsReadOnly();

    /// <summary>
    /// Gets all invitations (pending, accepted, declined, and revoked) associated
    /// with this watch space as a read-only list.
    /// </summary>
    public IReadOnlyList<Invitation> Invitations => _invitations.AsReadOnly();

    // Required by EF Core
    private WatchSpace() { }

    private WatchSpace(WatchSpaceId id, string name, Guid createdByUserId, DateTime now)
    {
        Id = id;
        Name = name;
        CreatedByUserId = createdByUserId;
        CreatedAtUtc = now;
        UpdatedAtUtc = now;

        _members.Add(new WatchSpaceMember(id, createdByUserId, WatchSpaceRole.Owner, now));
    }

    // --- Factory ---

    /// <summary>
    /// Creates a new watch space with the specified name. The creating user is
    /// automatically added as the first member with <see cref="WatchSpaceRole.Owner"/> role.
    /// </summary>
    /// <param name="name">
    /// The display name for the watch space. Must be non-empty and at most 100 characters.
    /// </param>
    /// <param name="createdByUserId">The identifier of the user creating the watch space.</param>
    /// <returns>A fully initialized <see cref="WatchSpace"/> aggregate with one owner member.</returns>
    /// <exception cref="WatchSpaceDomainException">
    /// Thrown when <paramref name="name"/> is empty, whitespace-only, or exceeds 100 characters.
    /// </exception>
    public static WatchSpace Create(string name, Guid createdByUserId)
    {
        ValidateName(name);
        return new WatchSpace(WatchSpaceId.New(), name, createdByUserId, DateTime.UtcNow);
    }

    // --- Commands ---

    /// <summary>
    /// Renames this watch space. Only an owner may perform this operation.
    /// </summary>
    /// <param name="newName">
    /// The new display name. Must be non-empty and at most 100 characters.
    /// </param>
    /// <param name="requestingUserId">The identifier of the user requesting the rename.</param>
    /// <exception cref="NotAnOwnerException">
    /// Thrown when <paramref name="requestingUserId"/> is not an owner of this watch space.
    /// </exception>
    /// <exception cref="WatchSpaceDomainException">
    /// Thrown when <paramref name="newName"/> is empty, whitespace-only, or exceeds 100 characters.
    /// </exception>
    public void Rename(string newName, Guid requestingUserId)
    {
        EnsureOwner(requestingUserId);
        ValidateName(newName);
        Name = newName;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a new invitation for the specified email address. Only an owner may invite members.
    /// A duplicate pending invitation for the same email is not allowed.
    /// </summary>
    /// <param name="invitedEmail">The email address of the person to invite.</param>
    /// <param name="invitedByUserId">The identifier of the owner sending the invitation.</param>
    /// <param name="expiresAtUtc">The UTC timestamp after which the invitation is no longer valid.</param>
    /// <returns>The newly created <see cref="Invitation"/> in <see cref="InvitationStatus.Pending"/> status.</returns>
    /// <exception cref="NotAnOwnerException">
    /// Thrown when <paramref name="invitedByUserId"/> is not an owner of this watch space.
    /// </exception>
    /// <exception cref="WatchSpaceDomainException">
    /// Thrown when a pending invitation already exists for <paramref name="invitedEmail"/>.
    /// </exception>
    public Invitation InviteMember(string invitedEmail, Guid invitedByUserId, DateTime expiresAtUtc)
    {
        EnsureOwner(invitedByUserId);

        if (_members.Any(m => m.UserId == Guid.Empty))
        {
            // placeholder -- real check done via IUserReadModel in application layer
        }

        var alreadyMember = _members.Any(m =>
            string.Equals(invitedEmail, GetMemberEmail(m), StringComparison.OrdinalIgnoreCase));

        var hasPending = _invitations.Any(i =>
            i.Status == InvitationStatus.Pending &&
            string.Equals(i.InvitedEmail, invitedEmail, StringComparison.OrdinalIgnoreCase));

        if (hasPending)
            throw new WatchSpaceDomainException($"A pending invitation already exists for '{invitedEmail}'.");

        var invitation = new Invitation(Id, invitedByUserId, invitedEmail, expiresAtUtc, DateTime.UtcNow);
        _invitations.Add(invitation);
        UpdatedAtUtc = DateTime.UtcNow;
        return invitation;
    }

    /// <summary>
    /// Accepts a pending invitation, adding the accepting user as a new member with
    /// <see cref="WatchSpaceRole.Member"/> role.
    /// </summary>
    /// <param name="token">The unique invitation token provided to the invitee.</param>
    /// <param name="acceptingUserId">The identifier of the user accepting the invitation.</param>
    /// <param name="acceptingEmail">
    /// The email address of the accepting user. Must match the invitation's target email
    /// (case-insensitive).
    /// </param>
    /// <param name="now">The current UTC timestamp, used for expiration checks and audit fields.</param>
    /// <returns>The newly created <see cref="WatchSpaceMember"/>.</returns>
    /// <exception cref="InvitationNotFoundException">
    /// Thrown when no invitation with the given <paramref name="token"/> exists.
    /// </exception>
    /// <exception cref="InvitationAlreadyProcessedException">
    /// Thrown when the invitation is no longer in <see cref="InvitationStatus.Pending"/> status.
    /// </exception>
    /// <exception cref="WatchSpaceDomainException">
    /// Thrown when <paramref name="acceptingEmail"/> does not match the invitation's email,
    /// or when the watch space has reached its maximum member capacity (20).
    /// </exception>
    /// <exception cref="InvitationExpiredException">
    /// Thrown when the invitation has expired as of <paramref name="now"/>.
    /// </exception>
    public WatchSpaceMember AcceptInvitation(string token, Guid acceptingUserId, string acceptingEmail, DateTime now)
    {
        var invitation = FindPendingInvitation(token);

        if (!string.Equals(invitation.InvitedEmail, acceptingEmail, StringComparison.OrdinalIgnoreCase))
            throw new WatchSpaceDomainException("This invitation was not sent to your email address.");

        if (invitation.IsExpired(now))
            throw new InvitationExpiredException();

        if (_members.Count >= MaxMembers)
            throw new WatchSpaceDomainException($"Watch space cannot exceed {MaxMembers} members.");

        invitation.Accept(now);

        var member = new WatchSpaceMember(Id, acceptingUserId, WatchSpaceRole.Member, now);
        _members.Add(member);
        UpdatedAtUtc = now;
        return member;
    }

    /// <summary>
    /// Declines a pending invitation on behalf of the invitee. The invitation transitions
    /// to <see cref="InvitationStatus.Declined"/> status.
    /// </summary>
    /// <param name="token">The unique invitation token.</param>
    /// <param name="decliningEmail">
    /// The email address of the person declining. Must match the invitation's target email
    /// (case-insensitive).
    /// </param>
    /// <param name="now">The current UTC timestamp for audit purposes.</param>
    /// <exception cref="InvitationNotFoundException">
    /// Thrown when no invitation with the given <paramref name="token"/> exists.
    /// </exception>
    /// <exception cref="WatchSpaceDomainException">
    /// Thrown when the invitation is not in <see cref="InvitationStatus.Pending"/> status,
    /// or when <paramref name="decliningEmail"/> does not match the invitation's email.
    /// </exception>
    public void DeclineInvitation(string token, string decliningEmail, DateTime now)
    {
        var invitation = _invitations.FirstOrDefault(i => i.Token == token)
            ?? throw new InvitationNotFoundException();

        if (invitation.Status != InvitationStatus.Pending)
            throw new WatchSpaceDomainException("Only pending invitations can be declined.");

        if (!string.Equals(invitation.InvitedEmail, decliningEmail, StringComparison.OrdinalIgnoreCase))
            throw new WatchSpaceDomainException("This invitation was not sent to your email address.");

        invitation.Decline();
        UpdatedAtUtc = now;
    }

    /// <summary>
    /// Revokes a pending invitation. Only an owner may revoke invitations. The invitation
    /// transitions to <see cref="InvitationStatus.Revoked"/> status.
    /// </summary>
    /// <param name="invitationId">The unique identifier of the invitation to revoke.</param>
    /// <param name="requestingUserId">The identifier of the owner performing the revocation.</param>
    /// <exception cref="NotAnOwnerException">
    /// Thrown when <paramref name="requestingUserId"/> is not an owner of this watch space.
    /// </exception>
    /// <exception cref="InvitationNotFoundException">
    /// Thrown when no invitation with the given <paramref name="invitationId"/> exists.
    /// </exception>
    /// <exception cref="WatchSpaceDomainException">
    /// Thrown when the invitation is not in <see cref="InvitationStatus.Pending"/> status.
    /// </exception>
    public void RevokeInvitation(Guid invitationId, Guid requestingUserId)
    {
        EnsureOwner(requestingUserId);

        var invitation = _invitations.FirstOrDefault(i => i.Id == invitationId)
            ?? throw new InvitationNotFoundException();

        if (invitation.Status != InvitationStatus.Pending)
            throw new WatchSpaceDomainException("Only pending invitations can be revoked.");

        invitation.Revoke();
        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Removes a member from this watch space. Only an owner may remove members.
    /// An owner cannot be removed directly; ownership must be transferred first.
    /// </summary>
    /// <param name="targetUserId">The identifier of the member to remove.</param>
    /// <param name="requestingUserId">The identifier of the owner performing the removal.</param>
    /// <exception cref="NotAnOwnerException">
    /// Thrown when <paramref name="requestingUserId"/> is not an owner of this watch space.
    /// </exception>
    /// <exception cref="MemberNotFoundException">
    /// Thrown when <paramref name="targetUserId"/> is not a member of this watch space.
    /// </exception>
    /// <exception cref="WatchSpaceDomainException">
    /// Thrown when attempting to remove a member who holds the <see cref="WatchSpaceRole.Owner"/> role.
    /// </exception>
    public void RemoveMember(Guid targetUserId, Guid requestingUserId)
    {
        EnsureOwner(requestingUserId);

        var target = _members.FirstOrDefault(m => m.UserId == targetUserId)
            ?? throw new MemberNotFoundException();

        if (target.Role == WatchSpaceRole.Owner)
            throw new WatchSpaceDomainException("Cannot remove an Owner directly. Transfer ownership first.");

        _members.Remove(target);
        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Allows a member to voluntarily leave this watch space. If the leaving member is an owner,
    /// at least one other owner must exist; otherwise the operation is rejected.
    /// </summary>
    /// <param name="leavingUserId">The identifier of the member who is leaving.</param>
    /// <exception cref="MemberNotFoundException">
    /// Thrown when <paramref name="leavingUserId"/> is not a member of this watch space.
    /// </exception>
    /// <exception cref="WatchSpaceDomainException">
    /// Thrown when the leaving member is the sole owner. Ownership must be transferred first.
    /// </exception>
    public void Leave(Guid leavingUserId)
    {
        var member = _members.FirstOrDefault(m => m.UserId == leavingUserId)
            ?? throw new MemberNotFoundException();

        if (member.Role == WatchSpaceRole.Owner)
            EnsureNotLastOwner();

        _members.Remove(member);
        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Transfers ownership from the requesting owner to another existing member.
    /// The current owner is demoted to <see cref="WatchSpaceRole.Member"/> and the
    /// target member is promoted to <see cref="WatchSpaceRole.Owner"/>.
    /// </summary>
    /// <param name="newOwnerId">The identifier of the member who will become the new owner.</param>
    /// <param name="requestingUserId">The identifier of the current owner initiating the transfer.</param>
    /// <exception cref="NotAnOwnerException">
    /// Thrown when <paramref name="requestingUserId"/> is not an owner of this watch space.
    /// </exception>
    /// <exception cref="WatchSpaceDomainException">
    /// Thrown when <paramref name="newOwnerId"/> is not a member of this watch space.
    /// </exception>
    public void TransferOwnership(Guid newOwnerId, Guid requestingUserId)
    {
        EnsureOwner(requestingUserId);

        var newOwner = _members.FirstOrDefault(m => m.UserId == newOwnerId)
            ?? throw new WatchSpaceDomainException("The specified user is not a member of this watch space.");

        var currentOwner = _members.First(m => m.UserId == requestingUserId);

        currentOwner.DemoteToMember();
        newOwner.PromoteToOwner();
        UpdatedAtUtc = DateTime.UtcNow;
    }

    // --- Helpers ---

    private void EnsureOwner(Guid userId)
    {
        var member = _members.FirstOrDefault(m => m.UserId == userId);
        if (member is null || member.Role != WatchSpaceRole.Owner)
            throw new NotAnOwnerException();
    }

    private void EnsureNotLastOwner()
    {
        var ownerCount = _members.Count(m => m.Role == WatchSpaceRole.Owner);
        if (ownerCount <= 1)
            throw new WatchSpaceDomainException(
                "Watch space must have at least one Owner. Transfer ownership before leaving.");
    }

    private Invitation FindPendingInvitation(string token)
    {
        var invitation = _invitations.FirstOrDefault(i => i.Token == token)
            ?? throw new InvitationNotFoundException();

        if (invitation.Status != InvitationStatus.Pending)
            throw new InvitationAlreadyProcessedException();

        return invitation;
    }

    // Email is managed externally; this method exists as a hook if needed later.
    private static string GetMemberEmail(WatchSpaceMember _) => string.Empty;

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new WatchSpaceDomainException("Watch space name cannot be empty.");
        if (name.Length > MaxNameLength)
            throw new WatchSpaceDomainException($"Watch space name cannot exceed {MaxNameLength} characters.");
    }
}

// Domain exceptions

/// <summary>
/// Thrown when a non-owner user attempts an operation that requires
/// <see cref="WatchSpaceRole.Owner"/> privileges (e.g., rename, invite, remove, revoke, transfer).
/// </summary>
public sealed class NotAnOwnerException()
    : WatchSpaceDomainException("Only an Owner can perform this action.");

/// <summary>
/// Thrown when an operation references a member who does not belong to the watch space.
/// </summary>
public sealed class MemberNotFoundException()
    : WatchSpaceDomainException("The specified user is not a member of this watch space.");

/// <summary>
/// Thrown when an operation references an invitation that does not exist in the watch space,
/// typically when an invalid or unknown token is provided.
/// </summary>
public sealed class InvitationNotFoundException()
    : WatchSpaceDomainException("Invitation not found.");

/// <summary>
/// Thrown when attempting to accept an invitation whose
/// <see cref="Invitation.ExpiresAtUtc"/> has passed.
/// </summary>
public sealed class InvitationExpiredException()
    : WatchSpaceDomainException("This invitation has expired.");

/// <summary>
/// Thrown when attempting to act on an invitation that is no longer in
/// <see cref="InvitationStatus.Pending"/> status (i.e., it has already been accepted,
/// declined, or revoked).
/// </summary>
public sealed class InvitationAlreadyProcessedException()
    : WatchSpaceDomainException("This invitation has already been accepted, declined, or revoked.");
