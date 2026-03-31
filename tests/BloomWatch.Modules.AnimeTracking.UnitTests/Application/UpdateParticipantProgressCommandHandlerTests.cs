using BloomWatch.Modules.AnimeTracking.Application.Abstractions;
using BloomWatch.Modules.AnimeTracking.Application.UseCases.UpdateParticipantProgress;
using BloomWatch.Modules.AnimeTracking.Domain.Aggregates;
using BloomWatch.Modules.AnimeTracking.Domain.Enums;
using BloomWatch.Modules.AnimeTracking.Domain.Exceptions;
using BloomWatch.Modules.AnimeTracking.Domain.Repositories;
using BloomWatch.Modules.AnimeTracking.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace BloomWatch.Modules.AnimeTracking.UnitTests.Application;

public sealed class UpdateParticipantProgressCommandHandlerTests
{
    private readonly IMembershipChecker _membershipChecker = Substitute.For<IMembershipChecker>();
    private readonly IAnimeTrackingRepository _repository = Substitute.For<IAnimeTrackingRepository>();
    private readonly UpdateParticipantProgressCommandHandler _handler;

    private readonly Guid _watchSpaceId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    public UpdateParticipantProgressCommandHandlerTests()
    {
        _handler = new UpdateParticipantProgressCommandHandler(_membershipChecker, _repository);
    }

    [Fact]
    public async Task HandleAsync_HappyPath_ReturnsUpdatedParticipantDetail()
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

        var command = new UpdateParticipantProgressCommand(
            _watchSpaceId, anime.Id.Value, _userId,
            IndividualStatus: AnimeStatus.Watching,
            EpisodesWatched: 5);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be(_userId);
        result.IndividualStatus.Should().Be("Watching");
        result.EpisodesWatched.Should().Be(5);
        result.RatingScore.Should().BeNull();
        result.RatingNotes.Should().BeNull();
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_NonMember_ThrowsNotAWatchSpaceMemberException()
    {
        // Arrange
        _membershipChecker.IsMemberAsync(_watchSpaceId, _userId, Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new UpdateParticipantProgressCommand(
            _watchSpaceId, Guid.NewGuid(), _userId,
            IndividualStatus: AnimeStatus.Watching,
            EpisodesWatched: 0);

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

        var command = new UpdateParticipantProgressCommand(
            _watchSpaceId, animeId.Value, _userId,
            IndividualStatus: AnimeStatus.Watching,
            EpisodesWatched: 0);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeNull();
        await _repository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
