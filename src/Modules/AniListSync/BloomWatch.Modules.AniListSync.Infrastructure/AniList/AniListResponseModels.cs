using System.Text.Json.Serialization;

namespace BloomWatch.Modules.AniListSync.Infrastructure.AniList;

/// <summary>
/// Represents the top-level JSON response returned by the AniList GraphQL API.
/// </summary>
internal sealed class AniListGraphQlResponse
{
    /// <summary>
    /// Gets or sets the <c>data</c> envelope of the GraphQL response.
    /// </summary>
    [JsonPropertyName("data")]
    public AniListData? Data { get; set; }
}

/// <summary>
/// Represents the <c>data</c> object within an AniList GraphQL response.
/// </summary>
internal sealed class AniListData
{
    /// <summary>
    /// Gets or sets the paginated result set returned by the <c>Page</c> query.
    /// </summary>
    [JsonPropertyName("Page")]
    public AniListPage? Page { get; set; }

    /// <summary>
    /// Gets or sets the single media entry returned by the <c>Media(id:)</c> query.
    /// </summary>
    [JsonPropertyName("Media")]
    public AniListMedia? Media { get; set; }
}

/// <summary>
/// Represents a single page of media results from the AniList GraphQL API.
/// </summary>
internal sealed class AniListPage
{
    /// <summary>
    /// Gets or sets the list of anime media entries on this page.
    /// </summary>
    [JsonPropertyName("media")]
    public List<AniListMedia>? Media { get; set; }
}

/// <summary>
/// Represents a single anime media entry returned by the AniList GraphQL API.
/// </summary>
internal sealed class AniListMedia
{
    /// <summary>
    /// Gets or sets the unique AniList media identifier.
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the title information for this media entry.
    /// </summary>
    [JsonPropertyName("title")]
    public AniListTitle? Title { get; set; }

    /// <summary>
    /// Gets or sets the cover image URLs for this media entry.
    /// </summary>
    [JsonPropertyName("coverImage")]
    public AniListCoverImage? CoverImage { get; set; }

    /// <summary>
    /// Gets or sets the total number of episodes, or <c>null</c> if unknown or still airing.
    /// </summary>
    [JsonPropertyName("episodes")]
    public int? Episodes { get; set; }

    /// <summary>
    /// Gets or sets the current airing status (e.g., "FINISHED", "RELEASING", "NOT_YET_RELEASED").
    /// </summary>
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    /// <summary>
    /// Gets or sets the media format (e.g., "TV", "MOVIE", "OVA", "ONA", "SPECIAL").
    /// </summary>
    [JsonPropertyName("format")]
    public string? Format { get; set; }

    /// <summary>
    /// Gets or sets the season in which the anime first aired (e.g., "WINTER", "SPRING", "SUMMER", "FALL").
    /// </summary>
    [JsonPropertyName("season")]
    public string? Season { get; set; }

    /// <summary>
    /// Gets or sets the year associated with the anime's airing season.
    /// </summary>
    [JsonPropertyName("seasonYear")]
    public int? SeasonYear { get; set; }

    /// <summary>
    /// Gets or sets the list of genre names associated with this media entry.
    /// </summary>
    [JsonPropertyName("genres")]
    public List<string>? Genres { get; set; }

    /// <summary>
    /// Gets or sets the HTML/Markdown description of the anime.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the average user score (0–100) on AniList.
    /// </summary>
    [JsonPropertyName("averageScore")]
    public int? AverageScore { get; set; }

    /// <summary>
    /// Gets or sets the popularity rank (number of users with this on their list).
    /// </summary>
    [JsonPropertyName("popularity")]
    public int? Popularity { get; set; }

    /// <summary>
    /// Gets or sets the list of tags associated with this media entry.
    /// </summary>
    [JsonPropertyName("tags")]
    public List<AniListTag>? Tags { get; set; }

    /// <summary>
    /// Gets or sets the URL of the media's page on AniList.
    /// </summary>
    [JsonPropertyName("siteUrl")]
    public string? SiteUrl { get; set; }
}

/// <summary>
/// Represents the title variants for an AniList media entry.
/// </summary>
internal sealed class AniListTitle
{
    /// <summary>
    /// Gets or sets the romanized (romaji) title.
    /// </summary>
    [JsonPropertyName("romaji")]
    public string? Romaji { get; set; }

    /// <summary>
    /// Gets or sets the English-localized title.
    /// </summary>
    [JsonPropertyName("english")]
    public string? English { get; set; }

    /// <summary>
    /// Gets or sets the native-language title.
    /// </summary>
    [JsonPropertyName("native")]
    public string? Native { get; set; }
}

/// <summary>
/// Represents the cover image URLs for an AniList media entry.
/// </summary>
internal sealed class AniListCoverImage
{
    /// <summary>
    /// Gets or sets the URL of the large-size cover image.
    /// </summary>
    [JsonPropertyName("large")]
    public string? Large { get; set; }
}

/// <summary>
/// Represents a single tag returned by the AniList GraphQL API.
/// </summary>
internal sealed class AniListTag
{
    /// <summary>
    /// Gets or sets the tag name (e.g., "Isekai", "Coming of Age").
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the tag's relevance rank (0–100).
    /// </summary>
    [JsonPropertyName("rank")]
    public int Rank { get; set; }

    /// <summary>
    /// Gets or sets whether this tag contains spoiler information.
    /// </summary>
    [JsonPropertyName("isMediaSpoiler")]
    public bool IsMediaSpoiler { get; set; }
}
