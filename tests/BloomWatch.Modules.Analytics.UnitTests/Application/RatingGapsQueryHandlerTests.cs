using BloomWatch.Modules.Analytics.Application.Abstractions;
using BloomWatch.Modules.Analytics.Application.DTOs;
using BloomWatch.Modules.Analytics.Application.UseCases.GetRatingGaps;
using FluentAssertions;
using NSubstitute;

namespace BloomWatch.Modules.Analytics.UnitTests.Application;

public sealed class RatingGapsQueryHandlerTests
{
    private readonly IMembershipChecker _membershipChecker = Substitute.For<IMembershipChecker>();
    private readonly IWatchSpaceAnalyticsDataSource _dataSource = Substitute.For<IWatchSpaceAnalyticsDataSource>();
    private readonly IUserDisplayNameLookup _displayNameLookup = Substitute.For<IUserDisplayNameLookup>();
    private readonly GetRatingGapsQueryHandler _handler;

    public RatingGapsQueryHandlerTests()
    {
        _handler = new GetRatingGapsQueryHandler(_membershipChecker, _dataSource, _displayNameLookup);
    }

    [Fact]
    public async Task HandleAsync_EqualGaps_TieBrokenByTitleAscending()
    {
        var watchSpaceId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var raterA = Guid.NewGuid();
        var raterB = Guid.NewGuid();

        _membershipChecker.IsMemberAsync(watchSpaceId, userId, Arg.Any<CancellationToken>())
            .Returns(true);

        // Both anime have gap = 3.0 (|8-5| = 3), but different titles
        var animeNaruto = new WatchSpaceAnimeData(
            Guid.NewGuid(), "Naruto", null, null, null, "Watching", 0, DateTime.UtcNow,
            new List<ParticipantData> { new(raterA, 8.0m), new(raterB, 5.0m) });

        var animeBleach = new WatchSpaceAnimeData(
            Guid.NewGuid(), "Bleach", null, null, null, "Watching", 0, DateTime.UtcNow,
            new List<ParticipantData> { new(raterA, 9.0m), new(raterB, 6.0m) });

        _dataSource.GetAnimeWithParticipantsAsync(watchSpaceId, Arg.Any<CancellationToken>())
            .Returns(new List<WatchSpaceAnimeData> { animeNaruto, animeBleach });

        _displayNameLookup.GetDisplayNamesAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, string> { [raterA] = "Alice", [raterB] = "Bob" });

        var result = await _handler.HandleAsync(new GetRatingGapsQuery(watchSpaceId, userId));

        result.Items.Should().HaveCount(2);
        // Both have gap 3.0, so tie-break by title: Bleach before Naruto
        result.Items[0].PreferredTitle.Should().Be("Bleach");
        result.Items[1].PreferredTitle.Should().Be("Naruto");
        result.Message.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_NoQualifyingAnime_ReturnsEmptyWithMessage()
    {
        var watchSpaceId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _membershipChecker.IsMemberAsync(watchSpaceId, userId, Arg.Any<CancellationToken>())
            .Returns(true);

        _dataSource.GetAnimeWithParticipantsAsync(watchSpaceId, Arg.Any<CancellationToken>())
            .Returns(new List<WatchSpaceAnimeData>());

        var result = await _handler.HandleAsync(new GetRatingGapsQuery(watchSpaceId, userId));

        result.Items.Should().BeEmpty();
        result.Message.Should().Be("Not enough data");
    }

    [Fact]
    public async Task HandleAsync_ReturnsAllQualifyingAnime_NoCap()
    {
        var watchSpaceId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var raterA = Guid.NewGuid();
        var raterB = Guid.NewGuid();

        _membershipChecker.IsMemberAsync(watchSpaceId, userId, Arg.Any<CancellationToken>())
            .Returns(true);

        // Create 5 anime, all with 2 raters
        var allAnime = Enumerable.Range(1, 5).Select(i => new WatchSpaceAnimeData(
            Guid.NewGuid(), $"Anime {i}", null, null, null, "Watching", 0, DateTime.UtcNow,
            new List<ParticipantData> { new(raterA, 8.0m), new(raterB, (decimal)i) })).ToList();

        _dataSource.GetAnimeWithParticipantsAsync(watchSpaceId, Arg.Any<CancellationToken>())
            .Returns(allAnime);

        _displayNameLookup.GetDisplayNamesAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, string> { [raterA] = "A", [raterB] = "B" });

        var result = await _handler.HandleAsync(new GetRatingGapsQuery(watchSpaceId, userId));

        // All 5 should be returned (no cap like the dashboard's top-3)
        result.Items.Should().HaveCount(5);
    }
}
