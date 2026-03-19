namespace BloomWatch.Modules.WatchSpaces.Application.Abstractions;

/// <summary>
/// Resolves user display names from the Identity module's schema.
/// </summary>
public interface IUserDisplayNameLookup
{
    /// <summary>
    /// Returns a dictionary mapping user IDs to display names.
    /// </summary>
    Task<IReadOnlyDictionary<Guid, string>> GetDisplayNamesAsync(
        IEnumerable<Guid> userIds, CancellationToken cancellationToken = default);
}
