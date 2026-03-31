using BloomWatch.Modules.AniListSync.Application.Abstractions;
using BloomWatch.Modules.AniListSync.Application.UseCases.GetMediaDetail;
using BloomWatch.Modules.AniListSync.Domain.Entities;
using BloomWatch.Modules.AniListSync.Infrastructure.AniList;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace BloomWatch.Modules.AniListSync.UnitTests;

public sealed class GetMediaDetailQueryHandlerTests
{
    private readonly IAniListClient _client = Substitute.For<IAniListClient>();
    private readonly IMediaCacheRepository _cacheRepo = Substitute.For<IMediaCacheRepository>();
    private readonly GetMediaDetailQueryHandler _handler;

    public GetMediaDetailQueryHandlerTests()
    {
        _handler = new GetMediaDetailQueryHandler(_cacheRepo, _client);
    }

    private static AnimeMediaDetail CreateDetail(int id = 1) => new(
        AnilistMediaId: id,
        TitleRomaji: "Cowboy Bebop",
        TitleEnglish: "Cowboy Bebop",
        TitleNative: "カウボーイビバップ",
        CoverImageUrl: "https://img.example.com/1.jpg",
        Episodes: 26,
        Status: "FINISHED",
        Format: "TV",
        Season: "SPRING",
        SeasonYear: 1998,
        Genres: ["Action", "Sci-Fi"],
        Description: "A bounty hunter crew.",
        AverageScore: 86,
        Popularity: 200000,
        Tags: [],
        SiteUrl: null,
        CachedAt: default);

    private static MediaCacheEntry CreateCacheEntry(int id = 1, DateTime? cachedAt = null) =>
        MediaCacheEntry.Create(
            id, "Cowboy Bebop", "Cowboy Bebop", "カウボーイビバップ",
            "https://img.example.com/1.jpg", 26, "FINISHED", "TV", "SPRING", 1998,
            ["Action", "Sci-Fi"], "A bounty hunter crew.", 86, 200000,
            [], null,
            cachedAt ?? DateTime.UtcNow);

    [Fact]
    public async Task HandleAsync_CacheHitFresh_ReturnsCachedDataWithoutCallingAniList()
    {
        var entry = CreateCacheEntry(cachedAt: DateTime.UtcNow.AddHours(-1));
        _cacheRepo.GetByAnilistMediaIdAsync(1, Arg.Any<CancellationToken>()).Returns(entry);

        var result = await _handler.Handle(new GetMediaDetailQuery(1), CancellationToken.None);

        result.Should().NotBeNull();
        result!.AnilistMediaId.Should().Be(1);
        result.TitleRomaji.Should().Be("Cowboy Bebop");
        await _client.DidNotReceive().GetMediaByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_CacheMiss_FetchesFromAniListAndStores()
    {
        _cacheRepo.GetByAnilistMediaIdAsync(1, Arg.Any<CancellationToken>()).Returns((MediaCacheEntry?)null);
        _client.GetMediaByIdAsync(1, Arg.Any<CancellationToken>()).Returns(CreateDetail());

        var result = await _handler.Handle(new GetMediaDetailQuery(1), CancellationToken.None);

        result.Should().NotBeNull();
        result!.AnilistMediaId.Should().Be(1);
        await _cacheRepo.Received(1).UpsertAsync(Arg.Any<MediaCacheEntry>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_StaleCache_RefreshesFromAniList()
    {
        var stale = CreateCacheEntry(cachedAt: DateTime.UtcNow.AddHours(-25));
        _cacheRepo.GetByAnilistMediaIdAsync(1, Arg.Any<CancellationToken>()).Returns(stale);
        _client.GetMediaByIdAsync(1, Arg.Any<CancellationToken>()).Returns(CreateDetail());

        var result = await _handler.Handle(new GetMediaDetailQuery(1), CancellationToken.None);

        result.Should().NotBeNull();
        await _client.Received(1).GetMediaByIdAsync(1, Arg.Any<CancellationToken>());
        await _cacheRepo.Received(1).UpsertAsync(Arg.Any<MediaCacheEntry>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_AniListFailureWithStaleEntry_ThrowsAndPreservesCache()
    {
        var stale = CreateCacheEntry(cachedAt: DateTime.UtcNow.AddHours(-25));
        _cacheRepo.GetByAnilistMediaIdAsync(1, Arg.Any<CancellationToken>()).Returns(stale);
        _client.GetMediaByIdAsync(1, Arg.Any<CancellationToken>())
            .ThrowsAsync(new AniListApiException("AniList API returned HTTP 500."));

        var act = () => _handler.Handle(new GetMediaDetailQuery(1), CancellationToken.None);

        await act.Should().ThrowAsync<AniListApiException>();
        await _cacheRepo.DidNotReceive().UpsertAsync(Arg.Any<MediaCacheEntry>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_AniListReturnsNull_ReturnsNull()
    {
        _cacheRepo.GetByAnilistMediaIdAsync(999, Arg.Any<CancellationToken>()).Returns((MediaCacheEntry?)null);
        _client.GetMediaByIdAsync(999, Arg.Any<CancellationToken>()).Returns((AnimeMediaDetail?)null);

        var result = await _handler.Handle(new GetMediaDetailQuery(999), CancellationToken.None);

        result.Should().BeNull();
    }
}
