namespace BloomWatch.Modules.AnimeTracking.Application.UseCases.UpdateParticipantProgress;

/// <summary>
/// Request body for updating the calling user's individual progress on a tracked anime.
/// </summary>
/// <param name="IndividualStatus">The user's personal tracking status (Backlog, Watching, Finished, Paused, or Dropped).</param>
/// <param name="EpisodesWatched">The number of episodes the user has personally watched.</param>
public sealed record UpdateParticipantProgressRequest(
    string IndividualStatus,
    int EpisodesWatched);
