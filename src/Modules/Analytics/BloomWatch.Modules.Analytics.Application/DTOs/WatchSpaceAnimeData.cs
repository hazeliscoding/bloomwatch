namespace BloomWatch.Modules.Analytics.Application.DTOs;

/// <summary>
/// Read-only projection of a watch space anime with its participant entries.
/// Used by the analytics data source to pass data from the AnimeTracking schema.
/// </summary>
public sealed record WatchSpaceAnimeData(
    Guid WatchSpaceAnimeId,
    string PreferredTitle,
    string? CoverImageUrl,
    int? EpisodeCountSnapshot,
    string? Format,
    string SharedStatus,
    int SharedEpisodesWatched,
    DateTime AddedAtUtc,
    IReadOnlyList<ParticipantData> Participants,
    string? Mood = null,
    string? Vibe = null,
    string? Pitch = null);

/// <summary>
/// Read-only projection of a participant entry with rating information.
/// </summary>
public sealed record ParticipantData(
    Guid UserId,
    decimal? RatingScore);
