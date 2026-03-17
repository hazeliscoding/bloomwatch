using BloomWatch.Modules.AnimeTracking.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace BloomWatch.Modules.AnimeTracking.Infrastructure.CrossModule;

/// <summary>
/// Looks up cached AniList media metadata from the <c>anilist_sync.media_cache</c> table.
/// Resolves preferred title as: TitleEnglish > TitleRomaji > TitleNative.
/// </summary>
internal sealed class MediaCacheLookup(AniListMediaCacheReadDbContext dbContext) : IMediaCacheLookup
{
    public async Task<MediaCacheSnapshot?> GetByAnilistMediaIdAsync(
        int anilistMediaId,
        CancellationToken cancellationToken = default)
    {
        var row = await dbContext.MediaCache
            .FirstOrDefaultAsync(m => m.AnilistMediaId == anilistMediaId, cancellationToken);

        if (row is null) return null;

        var preferredTitle = row.TitleEnglish ?? row.TitleRomaji ?? row.TitleNative ?? "Unknown";

        return new MediaCacheSnapshot(
            PreferredTitle: preferredTitle,
            Episodes: row.Episodes,
            CoverImageUrl: row.CoverImageUrl,
            Format: row.Format,
            Season: row.Season,
            SeasonYear: row.SeasonYear);
    }
}
