using BloomWatch.Modules.AnimeTracking.Application.UseCases.UpdateParticipantProgress;
using BloomWatch.SharedKernel.CQRS;

namespace BloomWatch.Modules.AnimeTracking.Application.UseCases.UpdateParticipantRating;

/// <summary>
/// Command to submit or update the requesting user's personal rating for a tracked anime.
/// Creates the participant entry if it does not already exist.
/// </summary>
/// <param name="WatchSpaceId">The watch space containing the anime.</param>
/// <param name="WatchSpaceAnimeId">The tracked anime to rate.</param>
/// <param name="RequestingUserId">The user submitting the rating (must be a member).</param>
/// <param name="RatingScore">The numeric rating (0.5–10.0, in 0.5 increments).</param>
/// <param name="RatingNotes">Optional free-text notes, or <c>null</c>.</param>
/// <param name="UpdateNotes">Whether to update the notes field (distinguishes "not provided" from "set to null").</param>
public sealed record UpdateParticipantRatingCommand(
    Guid WatchSpaceId,
    Guid WatchSpaceAnimeId,
    Guid RequestingUserId,
    decimal RatingScore,
    string? RatingNotes,
    bool UpdateNotes) : ICommand<ParticipantDetailResult?>;
