using BloomWatch.Modules.WatchSpaces.Application.Abstractions;
using BloomWatch.Modules.WatchSpaces.Domain.Repositories;
using MediatR;

namespace BloomWatch.Modules.WatchSpaces.Application.UseCases.GetMyWatchSpaces;

/// <summary>
/// Handles <see cref="GetMyWatchSpacesQuery"/> by retrieving all watch spaces
/// that the specified user belongs to and projecting them into summaries.
/// </summary>
/// <param name="repository">The watch space repository used for querying.</param>
/// <param name="displayNameLookup">Resolves user IDs to display names for member previews.</param>
public sealed class GetMyWatchSpacesQueryHandler(
    IWatchSpaceRepository repository,
    IUserDisplayNameLookup displayNameLookup)
    : IRequestHandler<GetMyWatchSpacesQuery, IReadOnlyList<WatchSpaceSummary>>
{
    /// <summary>
    /// Retrieves all watch spaces the user is a member of, along with their role in each.
    /// </summary>
    /// <param name="query">The query containing the user identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A read-only list of <see cref="WatchSpaceSummary"/> projections, one per watch space membership.</returns>
    public async Task<IReadOnlyList<WatchSpaceSummary>> Handle(
        GetMyWatchSpacesQuery query,
        CancellationToken cancellationToken)
    {
        var spaces = await repository.GetByMemberUserIdAsync(query.UserId, cancellationToken);

        var allMemberIds = spaces.SelectMany(ws => ws.Members.Select(m => m.UserId)).Distinct();
        var displayNames = await displayNameLookup.GetDisplayNamesAsync(allMemberIds, cancellationToken);

        return spaces
            .Select(ws =>
            {
                var member = ws.Members.First(m => m.UserId == query.UserId);
                var previews = ws.Members
                    .Select(m => new MemberPreview(displayNames.GetValueOrDefault(m.UserId, "Unknown")))
                    .ToList();
                return new WatchSpaceSummary(ws.Id.Value, ws.Name, ws.CreatedAtUtc, member.Role.ToString(), ws.Members.Count, previews);
            })
            .ToList();
    }
}
