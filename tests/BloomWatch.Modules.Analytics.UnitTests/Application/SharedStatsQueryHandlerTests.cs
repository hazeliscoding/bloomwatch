using BloomWatch.Modules.Analytics.Application.Abstractions;
using BloomWatch.Modules.Analytics.Application.DTOs;
using BloomWatch.Modules.Analytics.Application.Exceptions;
using BloomWatch.Modules.Analytics.Application.UseCases.GetSharedStats;
using FluentAssertions;
using NSubstitute;

namespace BloomWatch.Modules.Analytics.UnitTests.Application;

public sealed class SharedStatsQueryHandlerTests
{
    private readonly IMembershipChecker _membershipChecker = Substitute.For<IMembershipChecker>();
    private readonly IWatchSpaceAnalyticsDataSource _dataSource = Substitute.For<IWatchSpaceAnalyticsDataSource>();
    private readonly GetSharedStatsQueryHandler _handler;

    public SharedStatsQueryHandlerTests()
    {
        _handler = new GetSharedStatsQueryHandler(_membershipChecker, _dataSource);
    }

    [Fact]
    public async Task HandleAsync_MixedStatuses_ReturnsCorrectAggregates()
    {
        var watchSpaceId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _membershipChecker.IsMemberAsync(watchSpaceId, userId, Arg.Any<CancellationToken>())
            .Returns(true);

        var animeList = new List<WatchSpaceAnimeData>
        {
            new(Guid.NewGuid(), "Anime A", null, 24, "TV", "Finished", 24, DateTime.UtcNow, []),
            new(Guid.NewGuid(), "Anime B", null, 12, "TV", "Finished", 12, DateTime.UtcNow, []),
            new(Guid.NewGuid(), "Anime C", null, 25, "TV", "Watching", 10, DateTime.UtcNow, []),
            new(Guid.NewGuid(), "Anime D", null, 13, "TV", "Dropped", 5, DateTime.UtcNow, []),
            new(Guid.NewGuid(), "Anime E", null, 50, "TV", "Backlog", 0, DateTime.UtcNow, []),
        };

        _dataSource.GetAnimeWithParticipantsAsync(watchSpaceId, Arg.Any<CancellationToken>())
            .Returns(animeList);

        var result = await _handler.Handle(new GetSharedStatsQuery(watchSpaceId, userId), CancellationToken.None);

        result.TotalEpisodesWatchedTogether.Should().Be(51); // 24+12+10+5+0
        result.TotalFinished.Should().Be(2);
        result.TotalDropped.Should().Be(1);
    }

    [Fact]
    public async Task HandleAsync_EmptyWatchSpace_ReturnsZeroesAndNull()
    {
        var watchSpaceId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _membershipChecker.IsMemberAsync(watchSpaceId, userId, Arg.Any<CancellationToken>())
            .Returns(true);

        _dataSource.GetAnimeWithParticipantsAsync(watchSpaceId, Arg.Any<CancellationToken>())
            .Returns(new List<WatchSpaceAnimeData>());

        var result = await _handler.Handle(new GetSharedStatsQuery(watchSpaceId, userId), CancellationToken.None);

        result.TotalEpisodesWatchedTogether.Should().Be(0);
        result.TotalFinished.Should().Be(0);
        result.TotalDropped.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_NonMember_ThrowsNotAWatchSpaceMemberException()
    {
        var watchSpaceId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _membershipChecker.IsMemberAsync(watchSpaceId, userId, Arg.Any<CancellationToken>())
            .Returns(false);

        var act = () => _handler.Handle(new GetSharedStatsQuery(watchSpaceId, userId), CancellationToken.None);

        await act.Should().ThrowAsync<NotAWatchSpaceMemberException>();
    }
}
