using BloomWatch.Modules.AnimeTracking.Domain.Aggregates;
using BloomWatch.Modules.AnimeTracking.Domain.Enums;
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
