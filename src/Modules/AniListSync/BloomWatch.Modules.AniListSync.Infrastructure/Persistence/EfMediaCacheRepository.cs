using BloomWatch.Modules.AniListSync.Application.Abstractions;
using BloomWatch.Modules.AniListSync.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BloomWatch.Modules.AniListSync.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of <see cref="IMediaCacheRepository"/> using
/// <see cref="AniListSyncDbContext"/> for persistence.
/// </summary>
internal sealed class EfMediaCacheRepository : IMediaCacheRepository
{
    private readonly AniListSyncDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="EfMediaCacheRepository"/> class.
    /// </summary>
    public EfMediaCacheRepository(AniListSyncDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<MediaCacheEntry?> GetByAnilistMediaIdAsync(
        int anilistMediaId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.MediaCacheEntries
            .FindAsync([anilistMediaId], cancellationToken);
    }

    /// <inheritdoc />
    public async Task UpsertAsync(MediaCacheEntry entry, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.MediaCacheEntries
            .FindAsync([entry.AnilistMediaId], cancellationToken);

        if (existing is null)
        {
            _dbContext.MediaCacheEntries.Add(entry);
        }
        else if (!ReferenceEquals(existing, entry))
        {
            _dbContext.Entry(existing).CurrentValues.SetValues(entry);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
