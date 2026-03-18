namespace BloomWatch.Modules.AnimeTracking.Application.UseCases.UpdateSharedAnimeStatus;

public sealed record UpdateSharedAnimeStatusRequest(
    string? SharedStatus = null,
    int? SharedEpisodesWatched = null,
    string? Mood = null,
    string? Vibe = null,
    string? Pitch = null);
