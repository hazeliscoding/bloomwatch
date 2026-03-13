using BloomWatch.Modules.WatchSpaces.Domain.Aggregates;
using BloomWatch.Modules.WatchSpaces.Domain.ValueObjects;

namespace BloomWatch.Modules.WatchSpaces.Domain.Repositories;

public interface IWatchSpaceRepository
{
    Task<WatchSpace?> GetByIdAsync(WatchSpaceId id, CancellationToken cancellationToken = default);
    Task<WatchSpace?> GetByIdWithMembersAsync(WatchSpaceId id, CancellationToken cancellationToken = default);
    Task<WatchSpace?> GetByInvitationTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WatchSpace>> GetByMemberUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(WatchSpace watchSpace, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
