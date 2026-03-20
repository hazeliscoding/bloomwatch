namespace BloomWatch.Modules.AnimeTracking.Application.UseCases.UpdateParticipantProgress;

/// <summary>
/// Result returned after updating a participant's individual progress or rating for a tracked anime.
/// </summary>
/// <param name="UserId">The unique identifier of the participant.</param>
/// <param name="IndividualStatus">The participant's personal tracking status (e.g., "Watching", "Finished").</param>
/// <param name="EpisodesWatched">The number of episodes the participant has individually watched.</param>
/// <param name="RatingScore">The participant's numeric rating, or <c>null</c> if not yet rated.</param>
/// <param name="RatingNotes">The participant's free-text rating notes, or <c>null</c> if none.</param>
/// <param name="LastUpdatedAtUtc">The UTC timestamp of the last update to this participant entry.</param>
public sealed record ParticipantDetailResult(
    Guid UserId,
    string IndividualStatus,
    int EpisodesWatched,
    decimal? RatingScore,
    string? RatingNotes,
    DateTime LastUpdatedAtUtc);
