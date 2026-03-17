namespace BloomWatch.Modules.AnimeTracking.Application.UseCases.AddAnimeToWatchSpace;

/// <summary>
/// Command to add an anime to a watch space by its AniList media ID.
/// </summary>
public sealed record AddAnimeToWatchSpaceCommand(
    Guid WatchSpaceId,
    int AniListMediaId,
    string? Mood,
    string? Vibe,
    string? Pitch,
    Guid RequestingUserId);

/// <summary>
/// Result returned after successfully adding an anime to a watch space.
/// </summary>
public sealed record AddAnimeToWatchSpaceResult(
    Guid WatchSpaceAnimeId,
    string PreferredTitle,
    int? EpisodeCountSnapshot,
    string? CoverImageUrlSnapshot,
    string? Format,
    string? Season,
    int? SeasonYear);
