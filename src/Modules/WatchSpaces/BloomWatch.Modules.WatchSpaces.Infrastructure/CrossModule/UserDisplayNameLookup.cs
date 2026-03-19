using BloomWatch.Modules.WatchSpaces.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace BloomWatch.Modules.WatchSpaces.Infrastructure.CrossModule;

/// <summary>
/// Resolves user display names from the <c>identity.users</c> table.
/// </summary>
internal sealed class UserDisplayNameLookup(
    IdentityReadDbContext dbContext) : IUserDisplayNameLookup
{
    public async Task<IReadOnlyDictionary<Guid, string>> GetDisplayNamesAsync(
        IEnumerable<Guid> userIds, CancellationToken cancellationToken = default)
    {
        var idList = userIds.ToList();

        if (idList.Count == 0)
            return new Dictionary<Guid, string>();

        var rows = await dbContext.Users
            .Where(u => idList.Contains(u.UserId))
            .ToListAsync(cancellationToken);

        return rows.ToDictionary(u => u.UserId, u => u.DisplayName);
    }
}
