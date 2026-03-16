using BloomWatch.Modules.WatchSpaces.Domain.Enums;
using BloomWatch.Modules.WatchSpaces.Domain.ValueObjects;

namespace BloomWatch.Modules.WatchSpaces.Domain.Entities;

/// <summary>
/// Represents a user's membership in a <see cref="Aggregates.WatchSpace"/>.
/// <para>
/// A member is created either when the watch space is first created (the creator becomes
/// the <see cref="WatchSpaceRole.Owner"/>) or when an invitation is accepted (the invitee
/// joins as a <see cref="WatchSpaceRole.Member"/>).
/// </para>
/// <para>
/// This entity is owned by the <see cref="Aggregates.WatchSpace"/> aggregate root and must
/// not be created or mutated outside of it.
/// </para>
/// </summary>
public sealed class WatchSpaceMember
{
    /// <summary>
    /// Gets the unique identifier for this membership record.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the identifier of the watch space this membership belongs to.
    /// </summary>
    public WatchSpaceId WatchSpaceId { get; private set; }

    /// <summary>
    /// Gets the identifier of the user who holds this membership.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Gets the current role of this member within the watch space.
    /// The role may change via <see cref="PromoteToOwner"/> or <see cref="DemoteToMember"/>.
    /// </summary>
    public WatchSpaceRole Role { get; private set; }

    /// <summary>
    /// Gets the UTC timestamp of when this user joined the watch space.
    /// </summary>
    public DateTime JoinedAtUtc { get; private set; }

    // Required by EF Core
    private WatchSpaceMember() { }

    /// <summary>
    /// Initializes a new <see cref="WatchSpaceMember"/> with an auto-generated identifier.
    /// This constructor is internal because members must be created through the
    /// <see cref="Aggregates.WatchSpace"/> aggregate root.
    /// </summary>
    /// <param name="watchSpaceId">The identifier of the watch space to join.</param>
    /// <param name="userId">The identifier of the user becoming a member.</param>
    /// <param name="role">The initial role assigned to the member.</param>
    /// <param name="joinedAtUtc">The UTC timestamp of when the user joined.</param>
    internal WatchSpaceMember(WatchSpaceId watchSpaceId, Guid userId, WatchSpaceRole role, DateTime joinedAtUtc)
    {
        Id = Guid.NewGuid();
        WatchSpaceId = watchSpaceId;
        UserId = userId;
        Role = role;
        JoinedAtUtc = joinedAtUtc;
    }

    /// <summary>
    /// Demotes this member from <see cref="WatchSpaceRole.Owner"/> to
    /// <see cref="WatchSpaceRole.Member"/>. Called by the aggregate root during
    /// ownership transfer.
    /// </summary>
    internal void DemoteToMember() => Role = WatchSpaceRole.Member;

    /// <summary>
    /// Promotes this member from <see cref="WatchSpaceRole.Member"/> to
    /// <see cref="WatchSpaceRole.Owner"/>. Called by the aggregate root during
    /// ownership transfer.
    /// </summary>
    internal void PromoteToOwner() => Role = WatchSpaceRole.Owner;
}
