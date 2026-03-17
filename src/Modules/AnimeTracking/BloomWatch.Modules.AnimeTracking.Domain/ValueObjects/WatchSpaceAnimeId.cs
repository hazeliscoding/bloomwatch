namespace BloomWatch.Modules.AnimeTracking.Domain.ValueObjects;

/// <summary>
/// Strongly-typed identifier for a <see cref="Aggregates.WatchSpaceAnime"/> aggregate.
/// </summary>
public readonly record struct WatchSpaceAnimeId(Guid Value)
{
    public static WatchSpaceAnimeId New() => new(Guid.NewGuid());
    public static WatchSpaceAnimeId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}
