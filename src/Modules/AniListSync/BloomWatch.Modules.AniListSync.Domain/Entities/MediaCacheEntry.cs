namespace BloomWatch.Modules.AniListSync.Domain.Entities;

/// <summary>
/// Represents a cached AniList media entry stored in the <c>anilist_sync.media_cache</c> table.
/// </summary>
public sealed class MediaCacheEntry
{
    /// <summary>
    /// Gets the unique AniList media identifier (primary key).
    /// </summary>
    public int AnilistMediaId { get; private set; }

    /// <summary>
    /// Gets the romanized (romaji) title, or <c>null</c> if unavailable.
    /// </summary>
    public string? TitleRomaji { get; private set; }

    /// <summary>
    /// Gets the English-localized title, or <c>null</c> if unavailable.
    /// </summary>
    public string? TitleEnglish { get; private set; }

    /// <summary>
    /// Gets the native-language title, or <c>null</c> if unavailable.
    /// </summary>
    public string? TitleNative { get; private set; }

    /// <summary>
    /// Gets the URL of the large cover image, or <c>null</c> if unavailable.
    /// </summary>
    public string? CoverImageUrl { get; private set; }

    /// <summary>
    /// Gets the total number of episodes, or <c>null</c> if unknown.
    /// </summary>
    public int? Episodes { get; private set; }

    /// <summary>
    /// Gets the current airing status (e.g., "FINISHED", "RELEASING").
    /// </summary>
    public string? Status { get; private set; }

    /// <summary>
    /// Gets the media format (e.g., "TV", "MOVIE", "OVA").
    /// </summary>
    public string? Format { get; private set; }

    /// <summary>
    /// Gets the season in which the anime first aired (e.g., "WINTER", "SPRING").
    /// </summary>
    public string? Season { get; private set; }

    /// <summary>
    /// Gets the year associated with the anime's airing season.
    /// </summary>
    public int? SeasonYear { get; private set; }

    /// <summary>
    /// Gets the list of genre names associated with this media entry.
    /// </summary>
    public IReadOnlyList<string> Genres { get; private set; } = [];

    /// <summary>
    /// Gets the HTML/Markdown description of the anime, or <c>null</c> if unavailable.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Gets the average user score on AniList (0–100), or <c>null</c> if no scores exist.
    /// </summary>
    public int? AverageScore { get; private set; }

    /// <summary>
    /// Gets the popularity rank (number of users with this on their list), or <c>null</c>.
    /// </summary>
    public int? Popularity { get; private set; }

    /// <summary>
    /// Gets the UTC timestamp of when this entry was last fetched from AniList.
    /// </summary>
    public DateTime CachedAt { get; private set; }

    // Required by EF Core
    private MediaCacheEntry() { }

    /// <summary>
    /// Creates a new <see cref="MediaCacheEntry"/> with all metadata fields.
    /// </summary>
    public static MediaCacheEntry Create(
        int anilistMediaId,
        string? titleRomaji,
        string? titleEnglish,
        string? titleNative,
        string? coverImageUrl,
        int? episodes,
        string? status,
        string? format,
        string? season,
        int? seasonYear,
        IReadOnlyList<string> genres,
        string? description,
        int? averageScore,
        int? popularity,
        DateTime cachedAt)
    {
        return new MediaCacheEntry
        {
            AnilistMediaId = anilistMediaId,
            TitleRomaji = titleRomaji,
            TitleEnglish = titleEnglish,
            TitleNative = titleNative,
            CoverImageUrl = coverImageUrl,
            Episodes = episodes,
            Status = status,
            Format = format,
            Season = season,
            SeasonYear = seasonYear,
            Genres = genres,
            Description = description,
            AverageScore = averageScore,
            Popularity = popularity,
            CachedAt = cachedAt
        };
    }

    /// <summary>
    /// Updates all metadata fields and resets the <see cref="CachedAt"/> timestamp.
    /// </summary>
    public void Update(
        string? titleRomaji,
        string? titleEnglish,
        string? titleNative,
        string? coverImageUrl,
        int? episodes,
        string? status,
        string? format,
        string? season,
        int? seasonYear,
        IReadOnlyList<string> genres,
        string? description,
        int? averageScore,
        int? popularity,
        DateTime cachedAt)
    {
        TitleRomaji = titleRomaji;
        TitleEnglish = titleEnglish;
        TitleNative = titleNative;
        CoverImageUrl = coverImageUrl;
        Episodes = episodes;
        Status = status;
        Format = format;
        Season = season;
        SeasonYear = seasonYear;
        Genres = genres;
        Description = description;
        AverageScore = averageScore;
        Popularity = popularity;
        CachedAt = cachedAt;
    }
}
