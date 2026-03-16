using System.Net;
using System.Text;
using BloomWatch.Modules.AniListSync.Infrastructure.AniList;
using FluentAssertions;

namespace BloomWatch.Modules.AniListSync.UnitTests;

public sealed class AniListGraphQlClientGetMediaByIdTests
{
    private const string SampleMediaResponse = """
        {
          "data": {
            "Media": {
              "id": 1,
              "title": { "romaji": "Cowboy Bebop", "english": "Cowboy Bebop", "native": "カウボーイビバップ" },
              "coverImage": { "large": "https://img.example.com/1.jpg" },
              "episodes": 26,
              "status": "FINISHED",
              "format": "TV",
              "season": "SPRING",
              "seasonYear": 1998,
              "genres": ["Action", "Adventure", "Sci-Fi"],
              "description": "A bounty hunter crew in space.",
              "averageScore": 86,
              "popularity": 200000
            }
          }
        }
        """;

    private const string NullMediaResponse = """
        {
          "data": {
            "Media": null
          }
        }
        """;

    private static AniListGraphQlClient CreateClient(HttpStatusCode statusCode, string responseBody)
    {
        var handler = new StubHandler(statusCode, responseBody);
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://graphql.anilist.co")
        };
        return new AniListGraphQlClient(httpClient);
    }

    [Fact]
    public async Task GetMediaByIdAsync_SuccessfulResponse_MapsToAnimeMediaDetail()
    {
        var client = CreateClient(HttpStatusCode.OK, SampleMediaResponse);

        var result = await client.GetMediaByIdAsync(1);

        result.Should().NotBeNull();
        result!.AnilistMediaId.Should().Be(1);
        result.TitleRomaji.Should().Be("Cowboy Bebop");
        result.TitleEnglish.Should().Be("Cowboy Bebop");
        result.TitleNative.Should().Be("カウボーイビバップ");
        result.CoverImageUrl.Should().Be("https://img.example.com/1.jpg");
        result.Episodes.Should().Be(26);
        result.Status.Should().Be("FINISHED");
        result.Format.Should().Be("TV");
        result.Season.Should().Be("SPRING");
        result.SeasonYear.Should().Be(1998);
        result.Genres.Should().BeEquivalentTo(["Action", "Adventure", "Sci-Fi"]);
        result.Description.Should().Be("A bounty hunter crew in space.");
        result.AverageScore.Should().Be(86);
        result.Popularity.Should().Be(200000);
    }

    [Fact]
    public async Task GetMediaByIdAsync_UnknownId_ReturnsNull()
    {
        var client = CreateClient(HttpStatusCode.OK, NullMediaResponse);

        var result = await client.GetMediaByIdAsync(999999999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetMediaByIdAsync_HttpError_ThrowsAniListApiException()
    {
        var client = CreateClient(HttpStatusCode.InternalServerError, "");

        var act = () => client.GetMediaByIdAsync(1);

        await act.Should().ThrowAsync<AniListApiException>()
            .WithMessage("*HTTP 500*");
    }

    [Fact]
    public async Task GetMediaByIdAsync_MalformedResponse_ThrowsAniListApiException()
    {
        var client = CreateClient(HttpStatusCode.OK, "not json");

        var act = () => client.GetMediaByIdAsync(1);

        await act.Should().ThrowAsync<AniListApiException>()
            .WithMessage("*malformed*");
    }

    private sealed class StubHandler(HttpStatusCode statusCode, string responseBody) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(responseBody, Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
        }
    }
}
