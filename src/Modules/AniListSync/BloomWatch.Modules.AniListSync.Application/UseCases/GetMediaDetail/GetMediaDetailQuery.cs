namespace BloomWatch.Modules.AniListSync.Application.UseCases.GetMediaDetail;

/// <summary>
/// Represents a query to retrieve full cached metadata for a single AniList anime by its ID.
/// </summary>
/// <param name="AnilistMediaId">The AniList media identifier to look up.</param>
public sealed record GetMediaDetailQuery(int AnilistMediaId);
