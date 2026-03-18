using BloomWatch.Modules.AnimeTracking.Domain.Enums;

namespace BloomWatch.Modules.AnimeTracking.Application.UseCases.UpdateSharedAnimeStatus;

public sealed record UpdateSharedAnimeStatusCommand(
    Guid WatchSpaceId,
    Guid WatchSpaceAnimeId,
    Guid RequestingUserId,
    AnimeStatus? SharedStatus,
    int? SharedEpisodesWatched,
    string? Mood,
    string? Vibe,
    string? Pitch);
