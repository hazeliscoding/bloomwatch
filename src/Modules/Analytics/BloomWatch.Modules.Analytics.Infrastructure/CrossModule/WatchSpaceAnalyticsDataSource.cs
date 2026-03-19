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
                p.RatingScore)).ToList(),
            a.Mood,
            a.Vibe,
            a.Pitch)).ToList();
    }

    public async Task<(int Count, DateTime? MostRecentDate)> GetWatchSessionAggregateAsync(
        Guid watchSpaceId, CancellationToken cancellationToken = default)
    {
        var animeIds = await dbContext.Anime
            .AsNoTracking()
            .Where(a => a.WatchSpaceId == watchSpaceId)
            .Select(a => a.Id)
            .ToListAsync(cancellationToken);

        if (animeIds.Count == 0)
            return (0, null);

        var sessions = dbContext.WatchSessions
            .AsNoTracking()
            .Where(s => animeIds.Contains(s.WatchSpaceAnimeId));

        var count = await sessions.CountAsync(cancellationToken);
        var mostRecent = count > 0
            ? await sessions.MaxAsync(s => (DateTime?)s.SessionDateUtc, cancellationToken)
            : null;

        return (count, mostRecent);
    }
}
