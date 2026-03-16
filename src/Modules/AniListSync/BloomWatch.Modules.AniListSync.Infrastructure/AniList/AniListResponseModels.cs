using System.Text.Json.Serialization;

namespace BloomWatch.Modules.AniListSync.Infrastructure.AniList;

internal sealed class AniListGraphQlResponse
{
    [JsonPropertyName("data")]
    public AniListData? Data { get; set; }
}

internal sealed class AniListData
{
    [JsonPropertyName("Page")]
    public AniListPage? Page { get; set; }
}

internal sealed class AniListPage
{
    [JsonPropertyName("media")]
    public List<AniListMedia>? Media { get; set; }
}

internal sealed class AniListMedia
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public AniListTitle? Title { get; set; }

    [JsonPropertyName("coverImage")]
    public AniListCoverImage? CoverImage { get; set; }

    [JsonPropertyName("episodes")]
    public int? Episodes { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("format")]
    public string? Format { get; set; }

    [JsonPropertyName("season")]
    public string? Season { get; set; }

    [JsonPropertyName("seasonYear")]
    public int? SeasonYear { get; set; }

    [JsonPropertyName("genres")]
    public List<string>? Genres { get; set; }
}

internal sealed class AniListTitle
{
    [JsonPropertyName("romaji")]
    public string? Romaji { get; set; }

    [JsonPropertyName("english")]
    public string? English { get; set; }
}

internal sealed class AniListCoverImage
{
    [JsonPropertyName("large")]
    public string? Large { get; set; }
}
