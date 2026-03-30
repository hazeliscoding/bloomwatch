namespace BloomWatch.Modules.AniListSync.Domain.Entities;

/// <summary>
/// Represents a single AniList tag associated with a media entry.
/// </summary>
public sealed record MediaTag(string Name, int Rank, bool IsMediaSpoiler);
