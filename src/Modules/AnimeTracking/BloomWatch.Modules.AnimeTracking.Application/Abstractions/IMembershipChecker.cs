namespace BloomWatch.Modules.AnimeTracking.Application.Abstractions;

/// <summary>
/// Checks whether a user is a member of a given watch space.
/// Implemented in Infrastructure by querying the WatchSpaces module's schema.
/// </summary>
public interface IMembershipChecker
{
    Task<bool> IsMemberAsync(Guid watchSpaceId, Guid userId, CancellationToken cancellationToken = default);
}
