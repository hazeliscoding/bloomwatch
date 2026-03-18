using BloomWatch.Modules.AnimeTracking.Domain.Aggregates;
using BloomWatch.Modules.AnimeTracking.Domain.Enums;
using BloomWatch.Modules.AnimeTracking.Domain.Exceptions;
using FluentAssertions;

namespace BloomWatch.Modules.AnimeTracking.UnitTests.Domain;

public sealed class WatchSpaceAnimeTests
{
    private readonly Guid _watchSpaceId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    [Fact]
    public void Create_SetsAllMetadataSnapshots()
    {
        var anime = WatchSpaceAnime.Create(
            watchSpaceId: _watchSpaceId,
            aniListMediaId: 154587,
            preferredTitle: "Frieren: Beyond Journey's End",
            episodeCountSnapshot: 28,
            coverImageUrlSnapshot: "https://example.com/cover.jpg",
            format: "TV",
            season: "FALL",
            seasonYear: 2023,
            mood: "Cozy",
            vibe: "Sunday evening",
            pitch: "Best slow fantasy",
            addedByUserId: _userId);

        anime.Id.Value.Should().NotBeEmpty();
        anime.WatchSpaceId.Should().Be(_watchSpaceId);
        anime.AniListMediaId.Should().Be(154587);
        anime.PreferredTitle.Should().Be("Frieren: Beyond Journey's End");
        anime.EpisodeCountSnapshot.Should().Be(28);
        anime.CoverImageUrlSnapshot.Should().Be("https://example.com/cover.jpg");
        anime.Format.Should().Be("TV");
        anime.Season.Should().Be("FALL");
        anime.SeasonYear.Should().Be(2023);
        anime.Mood.Should().Be("Cozy");
        anime.Vibe.Should().Be("Sunday evening");
        anime.Pitch.Should().Be("Best slow fantasy");
        anime.AddedByUserId.Should().Be(_userId);
    }

    [Fact]
    public void Create_DefaultsSharedStatusToBacklog()
    {
        var anime = CreateDefault();

        anime.SharedStatus.Should().Be(AnimeStatus.Backlog);
        anime.SharedEpisodesWatched.Should().Be(0);
    }

    [Fact]
    public void Create_CreatesInitialParticipantEntry()
    {
        var anime = CreateDefault();

        anime.ParticipantEntries.Should().HaveCount(1);

        var entry = anime.ParticipantEntries[0];
        entry.UserId.Should().Be(_userId);
        entry.IndividualStatus.Should().Be(AnimeStatus.Backlog);
        entry.EpisodesWatched.Should().Be(0);
        entry.RatingScore.Should().BeNull();
        entry.RatingNotes.Should().BeNull();
        entry.WatchSpaceAnimeId.Should().Be(anime.Id);
    }

    [Fact]
    public void Create_SetsAddedAtUtcToRecentTime()
    {
        var before = DateTime.UtcNow;
        var anime = CreateDefault();
        var after = DateTime.UtcNow;

        anime.AddedAtUtc.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void Create_WithNullOptionalFields_Succeeds()
    {
        var anime = WatchSpaceAnime.Create(
            watchSpaceId: _watchSpaceId,
            aniListMediaId: 100,
            preferredTitle: "Test",
            episodeCountSnapshot: null,
            coverImageUrlSnapshot: null,
            format: null,
            season: null,
            seasonYear: null,
            mood: null,
            vibe: null,
            pitch: null,
            addedByUserId: _userId);

        anime.EpisodeCountSnapshot.Should().BeNull();
        anime.CoverImageUrlSnapshot.Should().BeNull();
        anime.Mood.Should().BeNull();
    }

    [Fact]
    public void UpdateSharedState_PartialUpdate_OnlyChangesProvidedFields()
    {
        var anime = CreateDefault();

        anime.UpdateSharedState(
            sharedStatus: AnimeStatus.Watching,
            sharedEpisodesWatched: 5,
            mood: null,
            vibe: null,
            pitch: null);

        anime.SharedStatus.Should().Be(AnimeStatus.Watching);
        anime.SharedEpisodesWatched.Should().Be(5);
        anime.Mood.Should().BeNull();
        anime.Vibe.Should().BeNull();
        anime.Pitch.Should().BeNull();
    }

    [Fact]
    public void UpdateSharedState_OnlyMetadata_LeavesStatusAndEpisodesUnchanged()
    {
        var anime = CreateDefault();

        anime.UpdateSharedState(
            sharedStatus: null,
            sharedEpisodesWatched: null,
            mood: "hype",
            vibe: "cozy nights",
            pitch: "a must-watch");

        anime.SharedStatus.Should().Be(AnimeStatus.Backlog);
        anime.SharedEpisodesWatched.Should().Be(0);
        anime.Mood.Should().Be("hype");
        anime.Vibe.Should().Be("cozy nights");
        anime.Pitch.Should().Be("a must-watch");
    }

    [Fact]
    public void UpdateSharedState_NegativeEpisodeCount_Throws()
    {
        var anime = CreateDefault();

        var act = () => anime.UpdateSharedState(
            sharedStatus: null,
            sharedEpisodesWatched: -1,
            mood: null,
            vibe: null,
            pitch: null);

        act.Should().Throw<InvalidSharedStateException>()
            .WithMessage("*non-negative*");
    }

    [Fact]
    public void UpdateSharedState_EpisodeCountExceedsSnapshot_Throws()
    {
        var anime = CreateDefault(); // episodeCountSnapshot = 28

        var act = () => anime.UpdateSharedState(
            sharedStatus: null,
            sharedEpisodesWatched: 29,
            mood: null,
            vibe: null,
            pitch: null);

        act.Should().Throw<InvalidSharedStateException>()
            .WithMessage("*cannot exceed*");
    }

    [Fact]
    public void UpdateSharedState_EpisodeCountWhenSnapshotNull_Accepted()
    {
        var anime = WatchSpaceAnime.Create(
            watchSpaceId: _watchSpaceId,
            aniListMediaId: 100,
            preferredTitle: "Ongoing",
            episodeCountSnapshot: null,
            coverImageUrlSnapshot: null,
            format: null,
            season: null,
            seasonYear: null,
            mood: null,
            vibe: null,
            pitch: null,
            addedByUserId: _userId);

        anime.UpdateSharedState(
            sharedStatus: null,
            sharedEpisodesWatched: 100,
            mood: null,
            vibe: null,
            pitch: null);

        anime.SharedEpisodesWatched.Should().Be(100);
    }

    [Fact]
    public void UpdateSharedState_EpisodeCountAtBoundary_Accepted()
    {
        var anime = CreateDefault(); // episodeCountSnapshot = 28

        anime.UpdateSharedState(
            sharedStatus: null,
            sharedEpisodesWatched: 28,
            mood: null,
            vibe: null,
            pitch: null);

        anime.SharedEpisodesWatched.Should().Be(28);
    }

    [Fact]
    public void UpdateParticipantProgress_ExistingEntry_UpdatesAndReturnsIt()
    {
        var anime = CreateDefault(); // creates initial entry for _userId
        var before = DateTime.UtcNow;

        var entry = anime.UpdateParticipantProgress(_userId, AnimeStatus.Watching, 7);

        entry.UserId.Should().Be(_userId);
        entry.IndividualStatus.Should().Be(AnimeStatus.Watching);
        entry.EpisodesWatched.Should().Be(7);
        entry.LastUpdatedAtUtc.Should().BeOnOrAfter(before);
        anime.ParticipantEntries.Should().HaveCount(1);
    }

    [Fact]
    public void UpdateParticipantProgress_NoExistingEntry_CreatesNewEntry()
    {
        var anime = CreateDefault();
        var otherUserId = Guid.NewGuid();

        var entry = anime.UpdateParticipantProgress(otherUserId, AnimeStatus.Watching, 3);

        entry.UserId.Should().Be(otherUserId);
        entry.IndividualStatus.Should().Be(AnimeStatus.Watching);
        entry.EpisodesWatched.Should().Be(3);
        anime.ParticipantEntries.Should().HaveCount(2);
    }

    [Fact]
    public void UpdateParticipantProgress_NegativeEpisodeCount_Throws()
    {
        var anime = CreateDefault();

        var act = () => anime.UpdateParticipantProgress(_userId, AnimeStatus.Watching, -1);

        act.Should().Throw<InvalidParticipantProgressException>()
            .WithMessage("*non-negative*");
    }

    [Fact]
    public void UpdateParticipantProgress_EpisodeCountExceedsSnapshot_Throws()
    {
        var anime = CreateDefault(); // episodeCountSnapshot = 28

        var act = () => anime.UpdateParticipantProgress(_userId, AnimeStatus.Watching, 29);

        act.Should().Throw<InvalidParticipantProgressException>()
            .WithMessage("*cannot exceed*");
    }

    [Fact]
    public void UpdateParticipantProgress_EpisodeCountWhenSnapshotNull_Accepted()
    {
        var anime = WatchSpaceAnime.Create(
            watchSpaceId: _watchSpaceId,
            aniListMediaId: 100,
            preferredTitle: "Ongoing",
            episodeCountSnapshot: null,
            coverImageUrlSnapshot: null,
            format: null,
            season: null,
            seasonYear: null,
            mood: null,
            vibe: null,
            pitch: null,
            addedByUserId: _userId);

        var entry = anime.UpdateParticipantProgress(_userId, AnimeStatus.Watching, 100);

        entry.EpisodesWatched.Should().Be(100);
    }

    private WatchSpaceAnime CreateDefault() => WatchSpaceAnime.Create(
        watchSpaceId: _watchSpaceId,
        aniListMediaId: 154587,
        preferredTitle: "Frieren",
        episodeCountSnapshot: 28,
        coverImageUrlSnapshot: "https://example.com/cover.jpg",
        format: "TV",
        season: "FALL",
        seasonYear: 2023,
        mood: null,
        vibe: null,
        pitch: null,
        addedByUserId: _userId);
}
