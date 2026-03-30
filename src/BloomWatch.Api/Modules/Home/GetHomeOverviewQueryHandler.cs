using BloomWatch.Modules.AnimeTracking.Domain.Enums;
using BloomWatch.Modules.AnimeTracking.Domain.Repositories;
using BloomWatch.Modules.Identity.Application.UseCases.GetProfile;
using BloomWatch.Modules.Identity.Domain.ValueObjects;
using BloomWatch.Modules.WatchSpaces.Application.Abstractions;
using BloomWatch.Modules.WatchSpaces.Domain.Repositories;

namespace BloomWatch.Api.Modules.Home;

/// <summary>
/// Thin orchestrator that aggregates data from Identity, WatchSpaces, and AnimeTracking
/// modules to build the home overview response.
/// </summary>
public sealed class GetHomeOverviewQueryHandler(
    GetUserProfileQueryHandler profileHandler,
    IWatchSpaceRepository watchSpaceRepository,
    IUserDisplayNameLookup displayNameLookup,
    IAnimeTrackingRepository animeTrackingRepository)
{
    public async Task<HomeOverviewResult> HandleAsync(
        GetHomeOverviewQuery query,
        CancellationToken cancellationToken = default)
    {
        // 1. Get display name
        var profile = await profileHandler.HandleAsync(
            new GetUserProfileQuery(UserId.From(query.UserId)), cancellationToken);

        // 2. Get user's watch spaces with members
        var spaces = await watchSpaceRepository.GetByMemberUserIdAsync(query.UserId, cancellationToken);

        // Resolve display names for all members
        var allMemberIds = spaces.SelectMany(ws => ws.Members.Select(m => m.UserId)).Distinct();
        var displayNames = await displayNameLookup.GetDisplayNamesAsync(allMemberIds, cancellationToken);

        // 3. For each watch space, get anime data for counts and recent activity
        var totalEpisodesTogether = 0;
        var allAniListIds = new HashSet<int>();
        var allAnimeForRecent = new List<(Guid WatchSpaceAnimeId, Guid WatchSpaceId, string WatchSpaceName,
            string PreferredTitle, string? CoverImageUrl, string SharedStatus, DateTime LastUpdated)>();

        var watchSpaceSummaries = new List<HomeWatchSpaceSummary>();

        foreach (var ws in spaces)
        {
            var animeList = await animeTrackingRepository.ListByWatchSpaceAsync(
                ws.Id.Value, null, cancellationToken);

            var watchingCount = 0;
            var backlogCount = 0;

            foreach (var anime in animeList)
            {
                allAniListIds.Add(anime.AniListMediaId);
                totalEpisodesTogether += anime.SharedEpisodesWatched;

                if (anime.SharedStatus == AnimeStatus.Watching) watchingCount++;
                if (anime.SharedStatus == AnimeStatus.Backlog) backlogCount++;

                // Derive last updated as max of AddedAtUtc and participant entry updates
                var lastUpdated = anime.AddedAtUtc;
                foreach (var entry in anime.ParticipantEntries)
                {
                    if (entry.LastUpdatedAtUtc > lastUpdated)
                        lastUpdated = entry.LastUpdatedAtUtc;
                }

                allAnimeForRecent.Add((
                    anime.Id.Value,
                    ws.Id.Value,
                    ws.Name,
                    anime.PreferredTitle,
                    anime.CoverImageUrlSnapshot,
                    anime.SharedStatus.ToString(),
                    lastUpdated));
            }

            var member = ws.Members.First(m => m.UserId == query.UserId);
            var previews = ws.Members
                .Take(5)
                .Select(m => new HomeMemberPreview(displayNames.GetValueOrDefault(m.UserId, "Unknown")))
                .ToList();

            watchSpaceSummaries.Add(new HomeWatchSpaceSummary(
                ws.Id.Value,
                ws.Name,
                member.Role.ToString(),
                ws.Members.Count,
                previews,
                watchingCount,
                backlogCount));
        }

        // 4. Build stats
        var stats = new HomeStatsResult(
            spaces.Count,
            allAniListIds.Count,
            totalEpisodesTogether);

        // 5. Build recent activity (top 3 most recently updated)
        var recentActivity = allAnimeForRecent
            .OrderByDescending(a => a.LastUpdated)
            .Take(3)
            .Select(a => new HomeRecentActivityItem(
                a.WatchSpaceAnimeId,
                a.WatchSpaceId,
                a.WatchSpaceName,
                a.PreferredTitle,
                a.CoverImageUrl,
                a.SharedStatus,
                a.LastUpdated))
            .ToList();

        // 6. Order watch spaces by most recently created
        watchSpaceSummaries = watchSpaceSummaries
            .OrderByDescending(ws => spaces.First(s => s.Id.Value == ws.WatchSpaceId).CreatedAtUtc)
            .ToList();

        return new HomeOverviewResult(
            profile.DisplayName,
            stats,
            watchSpaceSummaries,
            recentActivity);
    }
}
