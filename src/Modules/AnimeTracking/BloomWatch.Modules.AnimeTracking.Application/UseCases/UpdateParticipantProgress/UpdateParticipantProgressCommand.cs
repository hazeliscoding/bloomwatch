using BloomWatch.Modules.AnimeTracking.Domain.Enums;
using BloomWatch.SharedKernel.CQRS;

namespace BloomWatch.Modules.AnimeTracking.Application.UseCases.UpdateParticipantProgress;

/// <summary>
/// Command to update the requesting user's individual progress (status and episode count)
/// for a tracked anime. Creates the participant entry if it does not already exist.
/// </summary>
/// <param name="WatchSpaceId">The watch space containing the anime.</param>
/// <param name="WatchSpaceAnimeId">The tracked anime to update progress for.</param>
/// <param name="RequestingUserId">The user updating their progress (must be a member).</param>
/// <param name="IndividualStatus">The user's personal tracking status.</param>
/// <param name="EpisodesWatched">The number of episodes the user has individually watched.</param>
public sealed record UpdateParticipantProgressCommand(
    Guid WatchSpaceId,
    Guid WatchSpaceAnimeId,
    Guid RequestingUserId,
    AnimeStatus IndividualStatus,
    int EpisodesWatched) : ICommand<ParticipantDetailResult?>;
