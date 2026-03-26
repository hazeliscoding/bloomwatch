using BloomWatch.Modules.AnimeTracking.Domain.Aggregates;
using BloomWatch.Modules.AnimeTracking.Domain.Enums;
using BloomWatch.Modules.AnimeTracking.Domain.Repositories;
using BloomWatch.Modules.AnimeTracking.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace BloomWatch.Modules.AnimeTracking.Infrastructure.Persistence.Repositories;

internal sealed class EfAnimeTrackingRepository(AnimeTrackingDbContext dbContext) : IAnimeTrackingRepository
{
    public async Task AddAsync(WatchSpaceAnime anime, CancellationToken cancellationToken = default)
    {
        await dbContext.WatchSpaceAnimes.AddAsync(anime, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> ExistsAsync(Guid watchSpaceId, int aniListMediaId, CancellationToken cancellationToken = default)
        => dbContext.WatchSpaceAnimes.AnyAsync(
            a => a.WatchSpaceId == watchSpaceId && a.AniListMediaId == aniListMediaId,
            cancellationToken);

    public Task<WatchSpaceAnime?> GetByIdAsync(
        Guid watchSpaceId, WatchSpaceAnimeId id, CancellationToken cancellationToken = default)
        => dbContext.WatchSpaceAnimes
            .Include(a => a.ParticipantEntries)
            .FirstOrDefaultAsync(
                a => a.WatchSpaceId == watchSpaceId && a.Id == id,
                cancellationToken);

    public async Task<IReadOnlyList<WatchSpaceAnime>> ListByWatchSpaceAsync(
        Guid watchSpaceId, AnimeStatus? statusFilter, CancellationToken cancellationToken = default)
    {
        var query = dbContext.WatchSpaceAnimes
            .Include(a => a.ParticipantEntries)
            .Where(a => a.WatchSpaceId == watchSpaceId);

        if (statusFilter.HasValue)
            query = query.Where(a => a.SharedStatus == statusFilter.Value);

        return await query
            .OrderByDescending(a => a.AddedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => dbContext.SaveChangesAsync(cancellationToken);
}
