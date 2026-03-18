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

    // --- UpdateParticipantRating tests ---

    [Fact]
    public void UpdateParticipantRating_ExistingEntry_UpdatesRatingAndReturnsIt()
    {
        var anime = CreateDefault();
        var before = DateTime.UtcNow;

        var entry = anime.UpdateParticipantRating(_userId, 8.5m, "Great show", updateNotes: true);

        entry.UserId.Should().Be(_userId);
        entry.RatingScore.Should().Be(8.5m);
        entry.RatingNotes.Should().Be("Great show");
        entry.LastUpdatedAtUtc.Should().BeOnOrAfter(before);
        anime.ParticipantEntries.Should().HaveCount(1);
    }

    [Fact]
    public void UpdateParticipantRating_NoExistingEntry_CreatesNewEntryWithDefaults()
    {
        var anime = CreateDefault();
        var otherUserId = Guid.NewGuid();

        var entry = anime.UpdateParticipantRating(otherUserId, 7.0m, null, updateNotes: false);

        entry.UserId.Should().Be(otherUserId);
        entry.RatingScore.Should().Be(7.0m);
        entry.IndividualStatus.Should().Be(AnimeStatus.Backlog);
        entry.EpisodesWatched.Should().Be(0);
        anime.ParticipantEntries.Should().HaveCount(2);
    }

    [Fact]
    public void UpdateParticipantRating_BelowMinimum_Throws()
    {
        var anime = CreateDefault();

        var act = () => anime.UpdateParticipantRating(_userId, 0.0m, null, updateNotes: false);

        act.Should().Throw<InvalidRatingException>()
            .WithMessage("*between 0.5 and 10.0*");
    }

    [Fact]
    public void UpdateParticipantRating_AboveMaximum_Throws()
    {
        var anime = CreateDefault();

        var act = () => anime.UpdateParticipantRating(_userId, 10.5m, null, updateNotes: false);

        act.Should().Throw<InvalidRatingException>()
            .WithMessage("*between 0.5 and 10.0*");
    }

    [Fact]
    public void UpdateParticipantRating_NotInHalfIncrements_Throws()
    {
        var anime = CreateDefault();

        var act = () => anime.UpdateParticipantRating(_userId, 7.3m, null, updateNotes: false);

        act.Should().Throw<InvalidRatingException>()
            .WithMessage("*0.5 increments*");
    }

    [Fact]
    public void UpdateParticipantRating_NotesExceeding1000Characters_Throws()
    {
        var anime = CreateDefault();
        var longNotes = new string('x', 1001);

        var act = () => anime.UpdateParticipantRating(_userId, 8.0m, longNotes, updateNotes: true);

        act.Should().Throw<InvalidRatingException>()
            .WithMessage("*cannot exceed 1000*");
    }

    [Fact]
    public void UpdateParticipantRating_NullRatingNotes_ClearsExistingNotes()
    {
        var anime = CreateDefault();
        anime.UpdateParticipantRating(_userId, 8.0m, "Initial notes", updateNotes: true);

        var entry = anime.UpdateParticipantRating(_userId, 9.0m, null, updateNotes: true);

        entry.RatingNotes.Should().BeNull();
    }

    [Fact]
    public void UpdateParticipantRating_OmittedRatingNotes_PreservesExistingNotes()
    {
        var anime = CreateDefault();
        anime.UpdateParticipantRating(_userId, 8.0m, "Keep these notes", updateNotes: true);

        var entry = anime.UpdateParticipantRating(_userId, 9.0m, null, updateNotes: false);

        entry.RatingScore.Should().Be(9.0m);
        entry.RatingNotes.Should().Be("Keep these notes");
    }

    // --- RecordWatchSession tests ---

    [Fact]
    public void RecordWatchSession_ValidInput_CreatesSessionAndAddsToCollection()
    {
        var anime = CreateDefault();
        var sessionDate = new DateTime(2026, 3, 15, 20, 0, 0, DateTimeKind.Utc);

        var session = anime.RecordWatchSession(sessionDate, 1, 3, "Great first session!", _userId);

        session.Id.Should().NotBeEmpty();
        session.WatchSpaceAnimeId.Should().Be(anime.Id);
        session.SessionDateUtc.Should().Be(sessionDate);
        session.StartEpisode.Should().Be(1);
        session.EndEpisode.Should().Be(3);
        session.Notes.Should().Be("Great first session!");
        session.CreatedByUserId.Should().Be(_userId);
        anime.WatchSessions.Should().HaveCount(1);
    }

    [Fact]
    public void RecordWatchSession_SingleEpisode_Succeeds()
    {
        var anime = CreateDefault();

        var session = anime.RecordWatchSession(DateTime.UtcNow, 7, 7, null, _userId);

        session.StartEpisode.Should().Be(7);
        session.EndEpisode.Should().Be(7);
    }

    [Fact]
    public void RecordWatchSession_NullNotes_Succeeds()
    {
        var anime = CreateDefault();

        var session = anime.RecordWatchSession(DateTime.UtcNow, 1, 2, null, _userId);

        session.Notes.Should().BeNull();
    }

    [Fact]
    public void RecordWatchSession_StartEpisodeZero_Throws()
    {
        var anime = CreateDefault();

        var act = () => anime.RecordWatchSession(DateTime.UtcNow, 0, 3, null, _userId);

        act.Should().Throw<InvalidWatchSessionException>()
            .WithMessage("*at least 1*");
    }

    [Fact]
    public void RecordWatchSession_StartEpisodeNegative_Throws()
    {
        var anime = CreateDefault();

        var act = () => anime.RecordWatchSession(DateTime.UtcNow, -1, 3, null, _userId);

        act.Should().Throw<InvalidWatchSessionException>()
            .WithMessage("*at least 1*");
    }

    [Fact]
    public void RecordWatchSession_EndEpisodeLessThanStart_Throws()
    {
        var anime = CreateDefault();

        var act = () => anime.RecordWatchSession(DateTime.UtcNow, 5, 3, null, _userId);

        act.Should().Throw<InvalidWatchSessionException>()
            .WithMessage("*greater than or equal*");
    }

    [Fact]
    public void RecordWatchSession_MultipleSessions_AllAdded()
    {
        var anime = CreateDefault();

        anime.RecordWatchSession(DateTime.UtcNow, 1, 3, null, _userId);
        anime.RecordWatchSession(DateTime.UtcNow, 4, 6, null, _userId);

        anime.WatchSessions.Should().HaveCount(2);
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
