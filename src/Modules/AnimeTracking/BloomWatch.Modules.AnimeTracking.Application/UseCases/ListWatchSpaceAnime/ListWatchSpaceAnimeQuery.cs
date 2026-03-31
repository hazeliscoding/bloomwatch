using BloomWatch.Modules.AnimeTracking.Domain.Enums;
using BloomWatch.SharedKernel.CQRS;

namespace BloomWatch.Modules.AnimeTracking.Application.UseCases.ListWatchSpaceAnime;

/// <summary>
/// Query to list all anime tracked in a watch space, with optional status filter.
/// </summary>
public sealed record ListWatchSpaceAnimeQuery(
    Guid WatchSpaceId,
    AnimeStatus? Status,
    Guid RequestingUserId) : IQuery<ListWatchSpaceAnimeResult>;
