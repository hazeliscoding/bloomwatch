using BloomWatch.Modules.Analytics.Application.Abstractions;
using BloomWatch.Modules.Analytics.Application.DTOs;
using BloomWatch.Modules.Analytics.Application.Exceptions;
using BloomWatch.Modules.Analytics.Application.UseCases.GetRandomPick;
using FluentAssertions;
using NSubstitute;

namespace BloomWatch.Modules.Analytics.UnitTests.Application;

public sealed class RandomPickQueryHandlerTests
{
    private readonly IMembershipChecker _membershipChecker = Substitute.For<IMembershipChecker>();
    private readonly IWatchSpaceAnalyticsDataSource _dataSource = Substitute.For<IWatchSpaceAnalyticsDataSource>();
    private readonly GetRandomPickQueryHandler _handler;

    public RandomPickQueryHandlerTests()
    {
        _handler = new GetRandomPickQueryHandler(_membershipChecker, _dataSource);
    }

    [Fact]
    public async Task HandleAsync_NonMember_ThrowsNotAWatchSpaceMemberException()
    {
        var watchSpaceId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _membershipChecker.IsMemberAsync(watchSpaceId, userId, Arg.Any<CancellationToken>())
            .Returns(false);

        var act = () => _handler.HandleAsync(new GetRandomPickQuery(watchSpaceId, userId));

        await act.Should().ThrowAsync<NotAWatchSpaceMemberException>();
    }

    [Fact]
    public async Task HandleAsync_EmptyBacklog_ReturnsNullPickWithMessage()
    {
        var watchSpaceId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _membershipChecker.IsMemberAsync(watchSpaceId, userId, Arg.Any<CancellationToken>())
            .Returns(true);

        _dataSource.GetAnimeWithParticipantsAsync(watchSpaceId, Arg.Any<CancellationToken>())
            .Returns(new List<WatchSpaceAnimeData>());

        var result = await _handler.HandleAsync(new GetRandomPickQuery(watchSpaceId, userId));

        result.Pick.Should().BeNull();
        result.Message.Should().Be("Backlog is empty");
    }

    [Fact]
    public async Task HandleAsync_OnlyNonBacklogAnime_ReturnsNullPickWithMessage()
    {
        var watchSpaceId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _membershipChecker.IsMemberAsync(watchSpaceId, userId, Arg.Any<CancellationToken>())
            .Returns(true);

        var animeList = new List<WatchSpaceAnimeData>
        {
            new(Guid.NewGuid(), "Watching Anime", null, 12, "TV", "Watching", 5, DateTime.UtcNow, []),
            new(Guid.NewGuid(), "Finished Anime", null, 24, "TV", "Finished", 24, DateTime.UtcNow, []),
        };

        _dataSource.GetAnimeWithParticipantsAsync(watchSpaceId, Arg.Any<CancellationToken>())
            .Returns(animeList);

        var result = await _handler.HandleAsync(new GetRandomPickQuery(watchSpaceId, userId));

        result.Pick.Should().BeNull();
        result.Message.Should().Be("Backlog is empty");
    }

    [Fact]
    public async Task HandleAsync_BacklogWithAnime_ReturnsPickWithAllFields()
    {
        var watchSpaceId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var animeId = Guid.NewGuid();

        _membershipChecker.IsMemberAsync(watchSpaceId, userId, Arg.Any<CancellationToken>())
            .Returns(true);

        var animeList = new List<WatchSpaceAnimeData>
        {
            new(animeId, "My Backlog Anime", "https://img.example.com/cover.jpg", 24, "TV",
                "Backlog", 0, DateTime.UtcNow, [],
                Mood: "cozy", Vibe: "chill", Pitch: "Two friends watch anime together"),
        };

        _dataSource.GetAnimeWithParticipantsAsync(watchSpaceId, Arg.Any<CancellationToken>())
            .Returns(animeList);

        var result = await _handler.HandleAsync(new GetRandomPickQuery(watchSpaceId, userId));

        result.Pick.Should().NotBeNull();
        result.Message.Should().BeNull();
        result.Pick!.WatchSpaceAnimeId.Should().Be(animeId);
        result.Pick.PreferredTitle.Should().Be("My Backlog Anime");
        result.Pick.CoverImageUrlSnapshot.Should().Be("https://img.example.com/cover.jpg");
        result.Pick.EpisodeCountSnapshot.Should().Be(24);
        result.Pick.Mood.Should().Be("cozy");
        result.Pick.Vibe.Should().Be("chill");
        result.Pick.Pitch.Should().Be("Two friends watch anime together");
    }

    [Fact]
    public async Task HandleAsync_MixedStatuses_OnlyPicksFromBacklog()
    {
        var watchSpaceId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var backlogId = Guid.NewGuid();

        _membershipChecker.IsMemberAsync(watchSpaceId, userId, Arg.Any<CancellationToken>())
            .Returns(true);

        var animeList = new List<WatchSpaceAnimeData>
        {
            new(Guid.NewGuid(), "Watching", null, 12, "TV", "Watching", 5, DateTime.UtcNow, []),
            new(backlogId, "Backlog Pick", null, 13, "TV", "Backlog", 0, DateTime.UtcNow, []),
            new(Guid.NewGuid(), "Finished", null, 24, "TV", "Finished", 24, DateTime.UtcNow, []),
        };

        _dataSource.GetAnimeWithParticipantsAsync(watchSpaceId, Arg.Any<CancellationToken>())
            .Returns(animeList);

        var result = await _handler.HandleAsync(new GetRandomPickQuery(watchSpaceId, userId));

        result.Pick.Should().NotBeNull();
        result.Pick!.WatchSpaceAnimeId.Should().Be(backlogId);
    }
}
