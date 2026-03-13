using BloomWatch.Modules.WatchSpaces.Domain.Entities;
using BloomWatch.Modules.WatchSpaces.Domain.Enums;
using BloomWatch.Modules.WatchSpaces.Domain.Exceptions;
using BloomWatch.Modules.WatchSpaces.Domain.ValueObjects;

namespace BloomWatch.Modules.WatchSpaces.Domain.Aggregates;

public sealed class WatchSpace
{
    private const int MaxNameLength = 100;
    private const int MaxMembers = 20;

    private readonly List<WatchSpaceMember> _members = [];
    private readonly List<Invitation> _invitations = [];

    public WatchSpaceId Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public Guid CreatedByUserId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public IReadOnlyList<WatchSpaceMember> Members => _members.AsReadOnly();
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

    public static WatchSpace Create(string name, Guid createdByUserId)
    {
        ValidateName(name);
        return new WatchSpace(WatchSpaceId.New(), name, createdByUserId, DateTime.UtcNow);
    }

    // --- Commands ---

    public void Rename(string newName, Guid requestingUserId)
    {
        EnsureOwner(requestingUserId);
        ValidateName(newName);
        Name = newName;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public Invitation InviteMember(string invitedEmail, Guid invitedByUserId, DateTime expiresAtUtc)
    {
        EnsureOwner(invitedByUserId);

        if (_members.Any(m => m.UserId == Guid.Empty))
        {
            // placeholder — real check done via IUserReadModel in application layer
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

    public void Leave(Guid leavingUserId)
    {
        var member = _members.FirstOrDefault(m => m.UserId == leavingUserId)
            ?? throw new MemberNotFoundException();

        if (member.Role == WatchSpaceRole.Owner)
            EnsureNotLastOwner();

        _members.Remove(member);
        UpdatedAtUtc = DateTime.UtcNow;
    }

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
public sealed class NotAnOwnerException()
    : WatchSpaceDomainException("Only an Owner can perform this action.");

public sealed class MemberNotFoundException()
    : WatchSpaceDomainException("The specified user is not a member of this watch space.");

public sealed class InvitationNotFoundException()
    : WatchSpaceDomainException("Invitation not found.");

public sealed class InvitationExpiredException()
    : WatchSpaceDomainException("This invitation has expired.");

public sealed class InvitationAlreadyProcessedException()
    : WatchSpaceDomainException("This invitation has already been accepted, declined, or revoked.");
