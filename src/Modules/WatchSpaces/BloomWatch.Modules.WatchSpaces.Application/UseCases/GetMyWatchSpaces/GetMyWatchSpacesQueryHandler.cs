using BloomWatch.Modules.WatchSpaces.Domain.Repositories;

namespace BloomWatch.Modules.WatchSpaces.Application.UseCases.GetMyWatchSpaces;

/// <summary>
/// Handles <see cref="GetMyWatchSpacesQuery"/> by retrieving all watch spaces
/// that the specified user belongs to and projecting them into summaries.
/// </summary>
/// <param name="repository">The watch space repository used for querying.</param>
public sealed class GetMyWatchSpacesQueryHandler(IWatchSpaceRepository repository)
{
    /// <summary>
    /// Retrieves all watch spaces the user is a member of, along with their role in each.
    /// </summary>
    /// <param name="query">The query containing the user identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A read-only list of <see cref="WatchSpaceSummary"/> projections, one per watch space membership.</returns>
    public async Task<IReadOnlyList<WatchSpaceSummary>> HandleAsync(
        GetMyWatchSpacesQuery query,
        CancellationToken cancellationToken = default)
    {
        var spaces = await repository.GetByMemberUserIdAsync(query.UserId, cancellationToken);

        return spaces
            .Select(ws =>
            {
                var member = ws.Members.First(m => m.UserId == query.UserId);
                return new WatchSpaceSummary(ws.Id.Value, ws.Name, ws.CreatedAtUtc, member.Role.ToString());
            })
            .ToList();
    }
}
