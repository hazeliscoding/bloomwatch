using BloomWatch.Modules.Analytics.Application.Abstractions;
using BloomWatch.Modules.Analytics.Application.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BloomWatch.Modules.Analytics.Infrastructure.CrossModule;

/// <summary>
/// Reads anime and participant data from the <c>anime_tracking</c> schema
/// for analytics computations.
/// </summary>
internal sealed class WatchSpaceAnalyticsDataSource(
    AnimeTrackingReadDbContext dbContext) : IWatchSpaceAnalyticsDataSource
{
    public async Task<IReadOnlyList<WatchSpaceAnimeData>> GetAnimeWithParticipantsAsync(
        Guid watchSpaceId, CancellationToken cancellationToken = default)
    {
        var rows = await dbContext.Anime
            .AsNoTracking()
            .Include(a => a.Participants)
            .Where(a => a.WatchSpaceId == watchSpaceId)
            .ToListAsync(cancellationToken);

        return rows.Select(a => new WatchSpaceAnimeData(
            a.Id,
            a.PreferredTitle,
            a.CoverImageUrlSnapshot,
            a.EpisodeCountSnapshot,
            a.Format,
            a.SharedStatus,
            a.SharedEpisodesWatched,
            a.AddedAtUtc,
            a.Participants.Select(p => new ParticipantData(
                p.UserId,
                p.RatingScore)).ToList())).ToList();
    }
}
