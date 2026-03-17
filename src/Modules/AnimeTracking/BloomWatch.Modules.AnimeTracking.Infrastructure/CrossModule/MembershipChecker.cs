using BloomWatch.Modules.AnimeTracking.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace BloomWatch.Modules.AnimeTracking.Infrastructure.CrossModule;

/// <summary>
/// Checks watch space membership by querying the <c>watch_spaces.watch_space_members</c> table.
/// </summary>
internal sealed class MembershipChecker(WatchSpaceMembershipReadDbContext dbContext) : IMembershipChecker
{
    public Task<bool> IsMemberAsync(Guid watchSpaceId, Guid userId, CancellationToken cancellationToken = default)
        => dbContext.Members.AnyAsync(
            m => m.WatchSpaceId == watchSpaceId && m.UserId == userId,
            cancellationToken);
}
