namespace BloomWatch.Modules.AnimeTracking.Application.UseCases.UpdateParticipantProgress;

public sealed record UpdateParticipantProgressRequest(
    string IndividualStatus,
    int EpisodesWatched);
