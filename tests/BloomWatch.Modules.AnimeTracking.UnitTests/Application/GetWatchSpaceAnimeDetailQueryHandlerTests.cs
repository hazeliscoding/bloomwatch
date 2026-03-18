using BloomWatch.Modules.AnimeTracking.Application.Abstractions;
using BloomWatch.Modules.AnimeTracking.Application.UseCases.GetWatchSpaceAnimeDetail;
using BloomWatch.Modules.AnimeTracking.Domain.Aggregates;
using BloomWatch.Modules.AnimeTracking.Domain.Exceptions;
using BloomWatch.Modules.AnimeTracking.Domain.Repositories;
using BloomWatch.Modules.AnimeTracking.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace BloomWatch.Modules.AnimeTracking.UnitTests.Application;

public sealed class GetWatchSpaceAnimeDetailQueryHandlerTests
{
    private readonly IMembershipChecker _membershipChecker = Substitute.For<IMembershipChecker>();
    private readonly IAnimeTrackingRepository _repository = Substitute.For<IAnimeTrackingRepository>();
    private readonly GetWatchSpaceAnimeDetailQueryHandler _handler;

    private readonly Guid _watchSpaceId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    public GetWatchSpaceAnimeDetailQueryHandlerTests()
    {
        _handler = new GetWatchSpaceAnimeDetailQueryHandler(_membershipChecker, _repository);
    }

    [Fact]
    public async Task HandleAsync_WithExistingAnime_ReturnsFullDetail()
    {
        // Arrange
        _membershipChecker.IsMemberAsync(_watchSpaceId, _userId, Arg.Any<CancellationToken>())
            .Returns(true);

        var anime = WatchSpaceAnime.Create(
            _watchSpaceId, 154587, "Frieren: Beyond Journey's End",
            28, "https://example.com/cover.jpg", "TV", "FALL", 2023,
            "Cozy", "Sunday", "Best fantasy", _userId);

        _repository.GetByIdAsync(_watchSpaceId, anime.Id, Arg.Any<CancellationToken>())
            .Returns(anime);

        var query = new GetWatchSpaceAnimeDetailQuery(_watchSpaceId, anime.Id.Value, _userId);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result!.AnilistMediaId.Should().Be(154587);
        result.PreferredTitle.Should().Be("Frieren: Beyond Journey's End");
        result.Format.Should().Be("TV");
        result.Season.Should().Be("FALL");
        result.SeasonYear.Should().Be(2023);
        result.SharedStatus.Should().Be("Backlog");
        result.SharedEpisodesWatched.Should().Be(0);
        result.Mood.Should().Be("Cozy");
        result.Vibe.Should().Be("Sunday");
        result.Pitch.Should().Be("Best fantasy");
        result.AddedByUserId.Should().Be(_userId);
        result.Participants.Should().HaveCount(1);
        result.Participants[0].UserId.Should().Be(_userId);
        result.Participants[0].IndividualStatus.Should().Be("Backlog");
        result.Participants[0].EpisodesWatched.Should().Be(0);
        result.Participants[0].RatingScore.Should().BeNull();
        result.Participants[0].RatingNotes.Should().BeNull();
        result.WatchSessions.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_NonMember_ThrowsNotAWatchSpaceMemberException()
    {
        // Arrange
        _membershipChecker.IsMemberAsync(_watchSpaceId, _userId, Arg.Any<CancellationToken>())
            .Returns(false);

        var query = new GetWatchSpaceAnimeDetailQuery(_watchSpaceId, Guid.NewGuid(), _userId);

        // Act
        var act = () => _handler.HandleAsync(query);

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

        var query = new GetWatchSpaceAnimeDetailQuery(_watchSpaceId, animeId.Value, _userId);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().BeNull();
    }
}
