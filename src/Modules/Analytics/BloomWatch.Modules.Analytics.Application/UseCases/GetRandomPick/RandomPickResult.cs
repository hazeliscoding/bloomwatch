namespace BloomWatch.Modules.Analytics.Application.UseCases.GetRandomPick;

/// <summary>
/// Result of a random backlog pick, used to help the group decide what to watch next.
/// </summary>
/// <param name="Pick">The randomly selected backlog anime, or <c>null</c> if the backlog is empty.</param>
/// <param name="Message">A human-readable message when no pick is available (e.g., "Your backlog is empty — add some anime first!").</param>
public sealed record RandomPickResult(
    RandomPickAnimeResult? Pick,
    string? Message);

/// <summary>
/// Details of the randomly selected backlog anime.
/// </summary>
/// <param name="WatchSpaceAnimeId">The tracked anime's unique identifier within the watch space.</param>
/// <param name="PreferredTitle">The display title (English preferred, falling back to romaji).</param>
/// <param name="CoverImageUrlSnapshot">The cover image URL from the AniList snapshot, or <c>null</c>.</param>
/// <param name="EpisodeCountSnapshot">The total episode count from the AniList snapshot, or <c>null</c> if unknown/ongoing.</param>
/// <param name="Mood">The mood tag assigned when the anime was added, or <c>null</c>.</param>
/// <param name="Vibe">The vibe tag assigned when the anime was added, or <c>null</c>.</param>
/// <param name="Pitch">The pitch or reason for watching, or <c>null</c>.</param>
public sealed record RandomPickAnimeResult(
    Guid WatchSpaceAnimeId,
    string PreferredTitle,
    string? CoverImageUrlSnapshot,
    int? EpisodeCountSnapshot,
    string? Mood,
    string? Vibe,
    string? Pitch);
