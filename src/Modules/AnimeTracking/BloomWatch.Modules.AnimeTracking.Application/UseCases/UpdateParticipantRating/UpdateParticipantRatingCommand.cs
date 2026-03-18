namespace BloomWatch.Modules.AnimeTracking.Application.UseCases.UpdateParticipantRating;

public sealed record UpdateParticipantRatingCommand(
    Guid WatchSpaceId,
    Guid WatchSpaceAnimeId,
    Guid RequestingUserId,
    decimal RatingScore,
    string? RatingNotes,
    bool UpdateNotes);
