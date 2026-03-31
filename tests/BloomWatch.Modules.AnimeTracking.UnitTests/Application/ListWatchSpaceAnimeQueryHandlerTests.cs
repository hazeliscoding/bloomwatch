using BloomWatch.Modules.AnimeTracking.Application.Abstractions;
using BloomWatch.Modules.AnimeTracking.Application.UseCases.ListWatchSpaceAnime;
using BloomWatch.Modules.AnimeTracking.Domain.Aggregates;
using BloomWatch.Modules.AnimeTracking.Domain.Enums;
using BloomWatch.Modules.AnimeTracking.Domain.Exceptions;
using BloomWatch.Modules.AnimeTracking.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace BloomWatch.Modules.AnimeTracking.UnitTests.Application;

public sealed class ListWatchSpaceAnimeQueryHandlerTests
{
    private readonly IMembershipChecker _membershipChecker = Substitute.For<IMembershipChecker>();
    private readonly IAnimeTrackingRepository _repository = Substitute.For<IAnimeTrackingRepository>();
    private readonly ListWatchSpaceAnimeQueryHandler _handler;

    private readonly Guid _watchSpaceId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    public ListWatchSpaceAnimeQueryHandlerTests()
    {
        _handler = new ListWatchSpaceAnimeQueryHandler(_membershipChecker, _repository);
    }

    [Fact]
    public async Task HandleAsync_WithTrackedAnime_ReturnsItems()
    {
        // Arrange
        _membershipChecker.IsMemberAsync(_watchSpaceId, _userId, Arg.Any<CancellationToken>())
            .Returns(true);

        var anime = WatchSpaceAnime.Create(
            _watchSpaceId, 154587, "Frieren: Beyond Journey's End",
            28, "https://example.com/cover.jpg", "TV", "FALL", 2023,
            "Cozy", "Sunday", "Best fantasy", _userId);

        _repository.ListByWatchSpaceAsync(_watchSpaceId, null, Arg.Any<CancellationToken>())
            .Returns(new List<WatchSpaceAnime> { anime });

        var query = new ListWatchSpaceAnimeQuery(_watchSpaceId, null, _userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        var item = result.Items[0];
        item.AnilistMediaId.Should().Be(154587);
        item.PreferredTitle.Should().Be("Frieren: Beyond Journey's End");
        item.SharedStatus.Should().Be("Backlog");
        item.Participants.Should().HaveCount(1);
        item.Participants[0].UserId.Should().Be(_userId);
    }

    [Fact]
    public async Task HandleAsync_WithNoAnime_ReturnsEmptyList()
    {
        // Arrange
        _membershipChecker.IsMemberAsync(_watchSpaceId, _userId, Arg.Any<CancellationToken>())
            .Returns(true);
        _repository.ListByWatchSpaceAsync(_watchSpaceId, null, Arg.Any<CancellationToken>())
            .Returns(new List<WatchSpaceAnime>());

        var query = new ListWatchSpaceAnimeQuery(_watchSpaceId, null, _userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_NonMember_ThrowsNotAWatchSpaceMemberException()
    {
        // Arrange
        _membershipChecker.IsMemberAsync(_watchSpaceId, _userId, Arg.Any<CancellationToken>())
            .Returns(false);

        var query = new ListWatchSpaceAnimeQuery(_watchSpaceId, null, _userId);

        // Act
        var act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotAWatchSpaceMemberException>();
    }

    [Fact]
    public async Task HandleAsync_WithStatusFilter_PassesFilterToRepository()
    {
        // Arrange
        _membershipChecker.IsMemberAsync(_watchSpaceId, _userId, Arg.Any<CancellationToken>())
            .Returns(true);
        _repository.ListByWatchSpaceAsync(_watchSpaceId, AnimeStatus.Watching, Arg.Any<CancellationToken>())
            .Returns(new List<WatchSpaceAnime>());

        var query = new ListWatchSpaceAnimeQuery(_watchSpaceId, AnimeStatus.Watching, _userId);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        await _repository.Received(1).ListByWatchSpaceAsync(
            _watchSpaceId, AnimeStatus.Watching, Arg.Any<CancellationToken>());
    }
}
