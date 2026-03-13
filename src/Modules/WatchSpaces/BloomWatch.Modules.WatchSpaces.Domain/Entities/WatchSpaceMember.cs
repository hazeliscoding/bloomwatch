using BloomWatch.Modules.WatchSpaces.Domain.Enums;
using BloomWatch.Modules.WatchSpaces.Domain.ValueObjects;

namespace BloomWatch.Modules.WatchSpaces.Domain.Entities;

public sealed class WatchSpaceMember
{
    public Guid Id { get; private set; }
    public WatchSpaceId WatchSpaceId { get; private set; }
    public Guid UserId { get; private set; }
    public WatchSpaceRole Role { get; private set; }
    public DateTime JoinedAtUtc { get; private set; }

    // Required by EF Core
    private WatchSpaceMember() { }

    internal WatchSpaceMember(WatchSpaceId watchSpaceId, Guid userId, WatchSpaceRole role, DateTime joinedAtUtc)
    {
        Id = Guid.NewGuid();
        WatchSpaceId = watchSpaceId;
        UserId = userId;
        Role = role;
        JoinedAtUtc = joinedAtUtc;
    }

    internal void DemoteToMember() => Role = WatchSpaceRole.Member;
    internal void PromoteToOwner() => Role = WatchSpaceRole.Owner;
}
