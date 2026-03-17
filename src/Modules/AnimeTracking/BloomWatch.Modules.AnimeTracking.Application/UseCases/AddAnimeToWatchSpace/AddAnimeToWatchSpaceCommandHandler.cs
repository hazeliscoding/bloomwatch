using BloomWatch.Modules.AnimeTracking.Application.Abstractions;
using BloomWatch.Modules.AnimeTracking.Domain.Aggregates;
using BloomWatch.Modules.AnimeTracking.Domain.Exceptions;
using BloomWatch.Modules.AnimeTracking.Domain.Repositories;

namespace BloomWatch.Modules.AnimeTracking.Application.UseCases.AddAnimeToWatchSpace;

/// <summary>
/// Handles <see cref="AddAnimeToWatchSpaceCommand"/> by verifying membership,
/// checking for duplicates, looking up cached media metadata, and creating
/// the <see cref="WatchSpaceAnime"/> aggregate.
/// </summary>
public sealed class AddAnimeToWatchSpaceCommandHandler(
    IMembershipChecker membershipChecker,
    IMediaCacheLookup mediaCacheLookup,
    IAnimeTrackingRepository repository)
{
    public async Task<AddAnimeToWatchSpaceResult> HandleAsync(
        AddAnimeToWatchSpaceCommand command,
        CancellationToken cancellationToken = default)
    {
        // 1. Membership check
        var isMember = await membershipChecker.IsMemberAsync(
            command.WatchSpaceId, command.RequestingUserId, cancellationToken);

        if (!isMember)
            throw new NotAWatchSpaceMemberException();

        // 2. Duplicate check
        var exists = await repository.ExistsAsync(
            command.WatchSpaceId, command.AniListMediaId, cancellationToken);

        if (exists)
            throw new AnimeAlreadyInWatchSpaceException();

        // 3. Media cache lookup
        var media = await mediaCacheLookup.GetByAnilistMediaIdAsync(
            command.AniListMediaId, cancellationToken)
            ?? throw new MediaNotFoundException(command.AniListMediaId);

        // 4. Create aggregate
        var anime = WatchSpaceAnime.Create(
            watchSpaceId: command.WatchSpaceId,
            aniListMediaId: command.AniListMediaId,
            preferredTitle: media.PreferredTitle,
            episodeCountSnapshot: media.Episodes,
            coverImageUrlSnapshot: media.CoverImageUrl,
            format: media.Format,
            season: media.Season,
            seasonYear: media.SeasonYear,
            mood: command.Mood,
            vibe: command.Vibe,
            pitch: command.Pitch,
            addedByUserId: command.RequestingUserId);

        // 5. Persist
        await repository.AddAsync(anime, cancellationToken);

        return new AddAnimeToWatchSpaceResult(
            anime.Id.Value,
            anime.PreferredTitle,
            anime.EpisodeCountSnapshot,
            anime.CoverImageUrlSnapshot,
            anime.Format,
            anime.Season,
            anime.SeasonYear);
    }
}
