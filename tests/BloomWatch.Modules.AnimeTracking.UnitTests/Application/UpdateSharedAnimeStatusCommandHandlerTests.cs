using BloomWatch.Modules.AnimeTracking.Application.Abstractions;
using BloomWatch.Modules.AnimeTracking.Application.UseCases.UpdateSharedAnimeStatus;
using BloomWatch.Modules.AnimeTracking.Domain.Aggregates;
using BloomWatch.Modules.AnimeTracking.Domain.Enums;
using BloomWatch.Modules.AnimeTracking.Domain.Exceptions;
using BloomWatch.Modules.AnimeTracking.Domain.Repositories;
using BloomWatch.Modules.AnimeTracking.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace BloomWatch.Modules.AnimeTracking.UnitTests.Application;

public sealed class UpdateSharedAnimeStatusCommandHandlerTests
{
    private readonly IMembershipChecker _membershipChecker = Substitute.For<IMembershipChecker>();
    private readonly IAnimeTrackingRepository _repository = Substitute.For<IAnimeTrackingRepository>();
    private readonly UpdateSharedAnimeStatusCommandHandler _handler;

    private readonly Guid _watchSpaceId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    public UpdateSharedAnimeStatusCommandHandlerTests()
    {
        _handler = new UpdateSharedAnimeStatusCommandHandler(_membershipChecker, _repository);
    }

    [Fact]
    public async Task HandleAsync_HappyPath_ReturnsUpdatedDetail()
    {
        // Arrange
        _membershipChecker.IsMemberAsync(_watchSpaceId, _userId, Arg.Any<CancellationToken>())
            .Returns(true);

        var anime = WatchSpaceAnime.Create(
            _watchSpaceId, 154587, "Frieren", 28,
            "https://example.com/cover.jpg", "TV", "FALL", 2023,
            null, null, null, _userId);

        _repository.GetByIdAsync(_watchSpaceId, anime.Id, Arg.Any<CancellationToken>())
            .Returns(anime);

        var command = new UpdateSharedAnimeStatusCommand(
            _watchSpaceId, anime.Id.Value, _userId,
            SharedStatus: AnimeStatus.Watching,
            SharedEpisodesWatched: 5,
            Mood: "hype",
            Vibe: "cozy",
            Pitch: "must watch");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.SharedStatus.Should().Be("Watching");
        result.SharedEpisodesWatched.Should().Be(5);
        result.Mood.Should().Be("hype");
        result.Vibe.Should().Be("cozy");
        result.Pitch.Should().Be("must watch");
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_NonMember_ThrowsNotAWatchSpaceMemberException()
    {
        // Arrange
        _membershipChecker.IsMemberAsync(_watchSpaceId, _userId, Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new UpdateSharedAnimeStatusCommand(
            _watchSpaceId, Guid.NewGuid(), _userId,
            SharedStatus: AnimeStatus.Watching,
            SharedEpisodesWatched: null,
            Mood: null, Vibe: null, Pitch: null);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotAWatchSpaceMemberException>();
    }

    [Fact]
    public async Task HandleAsync_AnimeNotFound_ReturnsNull()
    {
        // Arrange
        _membershipChecker.IsMemberAsync(_watchSpaceId, _userId, Arg.Any<CancellationToken>())
            .Returns(true);

        var animeId = WatchSpaceAnimeId.New();
        _repository.GetByIdAsync(_watchSpaceId, animeId, Arg.Any<CancellationToken>())
            .Returns((WatchSpaceAnime?)null);

        var command = new UpdateSharedAnimeStatusCommand(
            _watchSpaceId, animeId.Value, _userId,
            SharedStatus: AnimeStatus.Watching,
            SharedEpisodesWatched: null,
            Mood: null, Vibe: null, Pitch: null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeNull();
        await _repository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
