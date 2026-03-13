using BloomWatch.Modules.WatchSpaces.Domain.Aggregates;
using BloomWatch.Modules.WatchSpaces.Domain.Entities;
using BloomWatch.Modules.WatchSpaces.Domain.Repositories;
using BloomWatch.Modules.WatchSpaces.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace BloomWatch.Modules.WatchSpaces.Infrastructure.Persistence.Repositories;

internal sealed class EfWatchSpaceRepository(WatchSpacesDbContext dbContext) : IWatchSpaceRepository
{
    public Task<WatchSpace?> GetByIdAsync(WatchSpaceId id, CancellationToken cancellationToken = default)
        => dbContext.WatchSpaces.FirstOrDefaultAsync(ws => ws.Id == id, cancellationToken);

    public Task<WatchSpace?> GetByIdWithMembersAsync(WatchSpaceId id, CancellationToken cancellationToken = default)
        => dbContext.WatchSpaces
            .Include(ws => ws.Members)
            .Include(ws => ws.Invitations)
            .FirstOrDefaultAsync(ws => ws.Id == id, cancellationToken);

    public async Task<WatchSpace?> GetByInvitationTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        // Find the WatchSpaceId via the Invitation table directly (avoids querying ignored navigation)
        var watchSpaceId = await dbContext.Set<Invitation>()
            .Where(i => i.Token == token)
            .Select(i => i.WatchSpaceId)
            .FirstOrDefaultAsync(cancellationToken);

        if (watchSpaceId == default) return null;

        return await GetByIdWithMembersAsync(watchSpaceId, cancellationToken);
    }

    public async Task<IReadOnlyList<WatchSpace>> GetByMemberUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Find WatchSpaceIds via the Member table directly (avoids querying ignored navigation)
        var watchSpaceIds = await dbContext.Set<WatchSpaceMember>()
            .Where(m => m.UserId == userId)
            .Select(m => m.WatchSpaceId)
            .ToListAsync(cancellationToken);

        return await dbContext.WatchSpaces
            .Include(ws => ws.Members)
            .Where(ws => watchSpaceIds.Contains(ws.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(WatchSpace watchSpace, CancellationToken cancellationToken = default)
    {
        await dbContext.WatchSpaces.AddAsync(watchSpace, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Accessing ChangeTracker.Entries() triggers DetectChanges automatically.
        // DetectChanges classifies entities with non-empty Guid keys found in backing-field
        // collections as "Modified" (assuming they were DB-loaded) rather than "Added".
        // Disabling auto-detect lets us safely iterate and explicitly mark new child
        // entities as Added BEFORE DetectChanges runs.
        dbContext.ChangeTracker.AutoDetectChangesEnabled = false;
        try
        {
            foreach (var entry in dbContext.ChangeTracker.Entries<WatchSpace>())
            {
                foreach (var member in entry.Entity.Members)
                    if (dbContext.Entry(member).State == EntityState.Detached)
                        dbContext.Set<WatchSpaceMember>().Add(member);

                foreach (var invitation in entry.Entity.Invitations)
                    if (dbContext.Entry(invitation).State == EntityState.Detached)
                        dbContext.Set<Invitation>().Add(invitation);
            }

            dbContext.ChangeTracker.DetectChanges();
        }
        finally
        {
            dbContext.ChangeTracker.AutoDetectChangesEnabled = true;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
