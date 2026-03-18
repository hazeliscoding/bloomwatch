using BloomWatch.Modules.AnimeTracking.Domain.Enums;

namespace BloomWatch.Modules.AnimeTracking.Application.UseCases.UpdateParticipantProgress;

public sealed record UpdateParticipantProgressCommand(
    Guid WatchSpaceId,
    Guid WatchSpaceAnimeId,
    Guid RequestingUserId,
    AnimeStatus IndividualStatus,
    int EpisodesWatched);
