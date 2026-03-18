namespace BloomWatch.Modules.AnimeTracking.Application.UseCases.UpdateParticipantProgress;

public sealed record ParticipantDetailResult(
    Guid UserId,
    string IndividualStatus,
    int EpisodesWatched,
    decimal? RatingScore,
    string? RatingNotes,
    DateTime LastUpdatedAtUtc);
