using BloomWatch.Modules.AnimeTracking.Domain.Aggregates;
using BloomWatch.Modules.AnimeTracking.Domain.Repositories;
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

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => dbContext.SaveChangesAsync(cancellationToken);
}
