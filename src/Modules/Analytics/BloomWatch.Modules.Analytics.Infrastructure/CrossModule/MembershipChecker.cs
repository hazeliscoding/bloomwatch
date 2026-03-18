using BloomWatch.Modules.Analytics.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace BloomWatch.Modules.Analytics.Infrastructure.CrossModule;

/// <summary>
/// Checks watch space membership by querying the <c>watch_spaces.watch_space_members</c> table.
/// </summary>
internal sealed class MembershipChecker(
    WatchSpaceMembershipReadDbContext dbContext) : IMembershipChecker
{
    public Task<bool> IsMemberAsync(Guid watchSpaceId, Guid userId, CancellationToken cancellationToken = default)
        => dbContext.Members.AnyAsync(
            m => m.WatchSpaceId == watchSpaceId && m.UserId == userId,
            cancellationToken);
}
