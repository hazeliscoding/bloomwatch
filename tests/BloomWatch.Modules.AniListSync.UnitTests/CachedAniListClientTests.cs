using BloomWatch.Modules.AniListSync.Application.Abstractions;
using BloomWatch.Modules.AniListSync.Application.UseCases.SearchAnime;
using BloomWatch.Modules.AniListSync.Infrastructure.AniList;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using NSubstitute;

namespace BloomWatch.Modules.AniListSync.UnitTests;

public sealed class CachedAniListClientTests : IDisposable
{
    private readonly IAniListClient _innerClient = Substitute.For<IAniListClient>();
    private readonly MemoryCache _cache = new(new MemoryCacheOptions());
    private readonly CachedAniListClient _sut;

    public CachedAniListClientTests()
    {
        _sut = new CachedAniListClient(_innerClient, _cache);
    }

    public void Dispose() => _cache.Dispose();

    [Fact]
    public async Task SearchAnimeAsync_CacheMiss_CallsInnerClient()
    {
        var expected = new List<AnimeSearchResult>
        {
            new(1, "Attack on Titan", "Attack on Titan", null, 25, "FINISHED", "TV", "SPRING", 2013, ["Action"])
        };

        _innerClient.SearchAnimeAsync("Attack on Titan", Arg.Any<CancellationToken>())
            .Returns(expected);

        var result = await _sut.SearchAnimeAsync("Attack on Titan");

        result.Should().BeEquivalentTo(expected);
        await _innerClient.Received(1).SearchAnimeAsync("Attack on Titan", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SearchAnimeAsync_CacheHit_DoesNotCallInnerClientAgain()
    {
        var expected = new List<AnimeSearchResult>
        {
            new(1, "Naruto", "Naruto", null, 220, "FINISHED", "TV", "FALL", 2002, ["Action"])
        };

        _innerClient.SearchAnimeAsync("Naruto", Arg.Any<CancellationToken>())
            .Returns(expected);

        await _sut.SearchAnimeAsync("Naruto");
        var result = await _sut.SearchAnimeAsync("Naruto");

        result.Should().BeEquivalentTo(expected);
        await _innerClient.Received(1).SearchAnimeAsync("Naruto", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SearchAnimeAsync_CaseInsensitive_ReturnsCachedResult()
    {
        var expected = new List<AnimeSearchResult>
        {
            new(1, "One Piece", "One Piece", null, null, "RELEASING", "TV", "FALL", 1999, ["Adventure"])
        };

        _innerClient.SearchAnimeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(expected);

        await _sut.SearchAnimeAsync("One Piece");
        var result = await _sut.SearchAnimeAsync("one piece");

        result.Should().BeEquivalentTo(expected);
        await _innerClient.Received(1).SearchAnimeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
