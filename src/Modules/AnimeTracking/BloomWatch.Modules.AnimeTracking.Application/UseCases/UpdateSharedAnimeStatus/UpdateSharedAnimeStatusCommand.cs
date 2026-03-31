using BloomWatch.Modules.AnimeTracking.Application.UseCases.GetWatchSpaceAnimeDetail;
using BloomWatch.Modules.AnimeTracking.Domain.Enums;
using BloomWatch.SharedKernel.CQRS;

namespace BloomWatch.Modules.AnimeTracking.Application.UseCases.UpdateSharedAnimeStatus;

/// <summary>
/// Command to partially update the shared tracking state (status, episodes, mood/vibe/pitch)
/// for an anime in a watch space. Only non-null parameters are applied.
/// </summary>
/// <param name="WatchSpaceId">The watch space containing the anime.</param>
/// <param name="WatchSpaceAnimeId">The tracked anime to update.</param>
/// <param name="RequestingUserId">The user performing the update (must be a member).</param>
/// <param name="SharedStatus">The new shared status, or <c>null</c> to leave unchanged.</param>
/// <param name="SharedEpisodesWatched">The new shared episode count, or <c>null</c> to leave unchanged.</param>
/// <param name="Mood">A mood tag, or <c>null</c> to leave unchanged.</param>
/// <param name="Vibe">A vibe tag, or <c>null</c> to leave unchanged.</param>
/// <param name="Pitch">A pitch/reason, or <c>null</c> to leave unchanged.</param>
public sealed record UpdateSharedAnimeStatusCommand(
    Guid WatchSpaceId,
    Guid WatchSpaceAnimeId,
    Guid RequestingUserId,
    AnimeStatus? SharedStatus,
    int? SharedEpisodesWatched,
    string? Mood,
    string? Vibe,
    string? Pitch) : ICommand<GetWatchSpaceAnimeDetailResult?>;
