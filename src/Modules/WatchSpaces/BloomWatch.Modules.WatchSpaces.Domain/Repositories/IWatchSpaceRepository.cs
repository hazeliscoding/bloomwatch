using BloomWatch.Modules.WatchSpaces.Domain.Aggregates;
using BloomWatch.Modules.WatchSpaces.Domain.ValueObjects;

namespace BloomWatch.Modules.WatchSpaces.Domain.Repositories;

/// <summary>
/// Repository interface for persisting and retrieving <see cref="WatchSpace"/> aggregates.
/// Implementations must load the aggregate with its full invariant boundary (members and
/// invitations) when the operation requires it.
/// </summary>
public interface IWatchSpaceRepository
{
    /// <summary>
    /// Retrieves a watch space by its identifier without eagerly loading related collections.
    /// Suitable for read-only queries that do not require member or invitation data.
    /// </summary>
    /// <param name="id">The unique identifier of the watch space to retrieve.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>
    /// The matching <see cref="WatchSpace"/>, or <see langword="null"/> if no watch space
    /// exists with the given <paramref name="id"/>.
    /// </returns>
    Task<WatchSpace?> GetByIdAsync(WatchSpaceId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a watch space by its identifier with members eagerly loaded.
    /// Use this method when performing commands that enforce membership invariants
    /// (e.g., adding or removing members, ownership transfer).
    /// </summary>
    /// <param name="id">The unique identifier of the watch space to retrieve.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>
    /// The matching <see cref="WatchSpace"/> with its <see cref="WatchSpace.Members"/>
    /// collection populated, or <see langword="null"/> if not found.
    /// </returns>
    Task<WatchSpace?> GetByIdWithMembersAsync(WatchSpaceId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the watch space that contains an invitation with the specified token.
    /// Used during invitation acceptance and decline flows where the caller only has the token.
    /// </summary>
    /// <param name="token">The unique invitation token to search for.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>
    /// The <see cref="WatchSpace"/> containing the matching invitation, or
    /// <see langword="null"/> if no invitation with the given token exists.
    /// </returns>
    Task<WatchSpace?> GetByInvitationTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all watch spaces in which the specified user is a member.
    /// </summary>
    /// <param name="userId">The identifier of the user whose watch spaces to retrieve.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>
    /// A read-only list of <see cref="WatchSpace"/> aggregates the user belongs to.
    /// Returns an empty list if the user is not a member of any watch space.
    /// </returns>
    Task<IReadOnlyList<WatchSpace>> GetByMemberUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists a new <see cref="WatchSpace"/> aggregate to the data store.
    /// </summary>
    /// <param name="watchSpace">The watch space aggregate to add.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    Task AddAsync(WatchSpace watchSpace, CancellationToken cancellationToken = default);

    /// <summary>
    /// Flushes all tracked changes for watch space aggregates to the data store.
    /// Call this after performing one or more domain operations to persist their effects.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
