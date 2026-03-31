using BloomWatch.Modules.AniListSync.Application.Abstractions;
using BloomWatch.Modules.AniListSync.Application.UseCases.SearchAnime;
using FluentAssertions;
using NSubstitute;

namespace BloomWatch.Modules.AniListSync.UnitTests;

public sealed class SearchAnimeQueryHandlerTests
{
    private readonly IAniListClient _client = Substitute.For<IAniListClient>();
    private readonly SearchAnimeQueryHandler _handler;

    public SearchAnimeQueryHandlerTests()
    {
        _handler = new SearchAnimeQueryHandler(_client);
    }

    [Fact]
    public async Task HandleAsync_ValidQuery_DelegatesToClient()
    {
        var expected = new List<AnimeSearchResult>
        {
            new(1, "Cowboy Bebop", "Cowboy Bebop", "https://img.example.com/1.jpg",
                26, "FINISHED", "TV", "SPRING", 1998, ["Action", "Sci-Fi"])
        };

        _client.SearchAnimeAsync("Cowboy Bebop", Arg.Any<CancellationToken>())
            .Returns(expected);

        var result = await _handler.Handle(new SearchAnimeQuery("Cowboy Bebop"), CancellationToken.None);

        result.Should().BeEquivalentTo(expected);
        await _client.Received(1).SearchAnimeAsync("Cowboy Bebop", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_TrimsQuery_BeforeDelegating()
    {
        _client.SearchAnimeAsync("Naruto", Arg.Any<CancellationToken>())
            .Returns(new List<AnimeSearchResult>());

        await _handler.Handle(new SearchAnimeQuery("  Naruto  "), CancellationToken.None);

        await _client.Received(1).SearchAnimeAsync("Naruto", Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task HandleAsync_EmptyOrWhitespaceQuery_ThrowsArgumentException(string? query)
    {
        var act = () => _handler.Handle(new SearchAnimeQuery(query!), CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("query");
    }
}
