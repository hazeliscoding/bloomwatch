using BloomWatch.Modules.WatchSpaces.Domain.Enums;
using BloomWatch.Modules.WatchSpaces.Domain.ValueObjects;

namespace BloomWatch.Modules.WatchSpaces.Domain.Entities;

public sealed class Invitation
{
    public Guid Id { get; private set; }
    public WatchSpaceId WatchSpaceId { get; private set; }
    public Guid InvitedByUserId { get; private set; }
    public string InvitedEmail { get; private set; } = string.Empty;
    public string Token { get; private set; } = string.Empty;
    public InvitationStatus Status { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? AcceptedAtUtc { get; private set; }

    // Required by EF Core
    private Invitation() { }

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

    internal void Accept(DateTime acceptedAtUtc)
    {
        Status = InvitationStatus.Accepted;
        AcceptedAtUtc = acceptedAtUtc;
    }

    internal void Decline() => Status = InvitationStatus.Declined;
    internal void Revoke() => Status = InvitationStatus.Revoked;

    public bool IsExpired(DateTime now) => ExpiresAtUtc < now;
}
