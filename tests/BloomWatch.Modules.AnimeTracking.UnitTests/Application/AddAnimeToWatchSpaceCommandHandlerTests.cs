using BloomWatch.Modules.AnimeTracking.Application.Abstractions;
using BloomWatch.Modules.AnimeTracking.Application.UseCases.AddAnimeToWatchSpace;
using BloomWatch.Modules.AnimeTracking.Domain.Aggregates;
using BloomWatch.Modules.AnimeTracking.Domain.Exceptions;
using BloomWatch.Modules.AnimeTracking.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace BloomWatch.Modules.AnimeTracking.UnitTests.Application;

public sealed class AddAnimeToWatchSpaceCommandHandlerTests
{
    private readonly IMembershipChecker _membershipChecker = Substitute.For<IMembershipChecker>();
    private readonly IMediaCacheLookup _mediaCacheLookup = Substitute.For<IMediaCacheLookup>();
    private readonly IAnimeTrackingRepository _repository = Substitute.For<IAnimeTrackingRepository>();
    private readonly AddAnimeToWatchSpaceCommandHandler _handler;

    private readonly Guid _watchSpaceId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();
    private const int AniListMediaId = 154587;

    public AddAnimeToWatchSpaceCommandHandlerTests()
    {
        _handler = new AddAnimeToWatchSpaceCommandHandler(
            _membershipChecker, _mediaCacheLookup, _repository);
    }

    [Fact]
    public async Task HandleAsync_HappyPath_ReturnsResultWithMetadata()
    {
        // Arrange
        _membershipChecker.IsMemberAsync(_watchSpaceId, _userId, Arg.Any<CancellationToken>())
            .Returns(true);
        _repository.ExistsAsync(_watchSpaceId, AniListMediaId, Arg.Any<CancellationToken>())
            .Returns(false);
        _mediaCacheLookup.GetByAnilistMediaIdAsync(AniListMediaId, Arg.Any<CancellationToken>())
            .Returns(new MediaCacheSnapshot(
                "Frieren: Beyond Journey's End", 28,
                "https://example.com/cover.jpg", "TV", "FALL", 2023));

        var command = new AddAnimeToWatchSpaceCommand(
            _watchSpaceId, AniListMediaId, "Cozy", "Sunday", "Best fantasy", _userId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.WatchSpaceAnimeId.Should().NotBeEmpty();
        result.PreferredTitle.Should().Be("Frieren: Beyond Journey's End");
        result.EpisodeCountSnapshot.Should().Be(28);
        result.Format.Should().Be("TV");

        await _repository.Received(1).AddAsync(
            Arg.Is<WatchSpaceAnime>(a =>
                a.WatchSpaceId == _watchSpaceId &&
                a.AniListMediaId == AniListMediaId &&
                a.Mood == "Cozy"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_NonMember_ThrowsNotAWatchSpaceMemberException()
    {
        _membershipChecker.IsMemberAsync(_watchSpaceId, _userId, Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new AddAnimeToWatchSpaceCommand(
            _watchSpaceId, AniListMediaId, null, null, null, _userId);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotAWatchSpaceMemberException>();
    }

    [Fact]
    public async Task HandleAsync_DuplicateAnime_ThrowsAnimeAlreadyInWatchSpaceException()
    {
        _membershipChecker.IsMemberAsync(_watchSpaceId, _userId, Arg.Any<CancellationToken>())
            .Returns(true);
        _repository.ExistsAsync(_watchSpaceId, AniListMediaId, Arg.Any<CancellationToken>())
            .Returns(true);

        var command = new AddAnimeToWatchSpaceCommand(
            _watchSpaceId, AniListMediaId, null, null, null, _userId);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<AnimeAlreadyInWatchSpaceException>();
    }

    [Fact]
    public async Task HandleAsync_MediaNotInCache_ThrowsMediaNotFoundException()
    {
        _membershipChecker.IsMemberAsync(_watchSpaceId, _userId, Arg.Any<CancellationToken>())
            .Returns(true);
        _repository.ExistsAsync(_watchSpaceId, AniListMediaId, Arg.Any<CancellationToken>())
            .Returns(false);
        _mediaCacheLookup.GetByAnilistMediaIdAsync(AniListMediaId, Arg.Any<CancellationToken>())
            .Returns((MediaCacheSnapshot?)null);

        var command = new AddAnimeToWatchSpaceCommand(
            _watchSpaceId, AniListMediaId, null, null, null, _userId);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<MediaNotFoundException>();
    }
}
