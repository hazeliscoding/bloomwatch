namespace BloomWatch.Modules.Analytics.Application.UseCases.GetRandomPick;

public sealed record RandomPickResult(
    RandomPickAnimeResult? Pick,
    string? Message);

public sealed record RandomPickAnimeResult(
    Guid WatchSpaceAnimeId,
    string PreferredTitle,
    string? CoverImageUrlSnapshot,
    int? EpisodeCountSnapshot,
    string? Mood,
    string? Vibe,
    string? Pitch);
