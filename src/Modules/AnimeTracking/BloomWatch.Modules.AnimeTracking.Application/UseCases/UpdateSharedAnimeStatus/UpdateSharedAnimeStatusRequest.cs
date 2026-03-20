namespace BloomWatch.Modules.AnimeTracking.Application.UseCases.UpdateSharedAnimeStatus;

/// <summary>
/// Request body for partially updating the shared tracking state of an anime in a watch space.
/// Only provided (non-null) fields are applied; omitted fields remain unchanged.
/// </summary>
/// <param name="SharedStatus">The new shared status value (Backlog, Watching, Finished, Paused, or Dropped), or <c>null</c> to leave unchanged.</param>
/// <param name="SharedEpisodesWatched">The new shared episode count, or <c>null</c> to leave unchanged.</param>
/// <param name="Mood">A free-text mood tag (e.g., "cozy", "intense"), or <c>null</c> to leave unchanged.</param>
/// <param name="Vibe">A free-text vibe tag (e.g., "late-night binge"), or <c>null</c> to leave unchanged.</param>
/// <param name="Pitch">A short pitch or reason for watching, or <c>null</c> to leave unchanged.</param>
public sealed record UpdateSharedAnimeStatusRequest(
    string? SharedStatus = null,
    int? SharedEpisodesWatched = null,
    string? Mood = null,
    string? Vibe = null,
    string? Pitch = null);
