using BloomWatch.Modules.WatchSpaces.Application.UseCases.RenameWatchSpace;
using BloomWatch.Modules.WatchSpaces.Domain.Aggregates;
using BloomWatch.Modules.WatchSpaces.Domain.Repositories;
using BloomWatch.Modules.WatchSpaces.Domain.ValueObjects;

namespace BloomWatch.Modules.WatchSpaces.Application.UseCases.GetWatchSpaceById;

/// <summary>
/// Handles <see cref="GetWatchSpaceByIdQuery"/> by loading the watch space with its members
/// and verifying the requesting user has access.
/// </summary>
/// <param name="repository">The watch space repository used for querying.</param>
public sealed class GetWatchSpaceByIdQueryHandler(IWatchSpaceRepository repository)
{
    /// <summary>
    /// Retrieves a watch space by its identifier, including the full member list.
    /// Only members of the watch space can access its details.
    /// </summary>
    /// <param name="query">The query containing the watch space identifier and requesting user.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="WatchSpaceDetail"/> containing the watch space metadata and member list.</returns>
    /// <exception cref="WatchSpaceNotFoundException">Thrown when no watch space exists with the given identifier.</exception>
    /// <exception cref="NotAMemberException">Thrown when the requesting user is not a member of the watch space.</exception>
    public async Task<WatchSpaceDetail> HandleAsync(
        GetWatchSpaceByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var watchSpace = await repository.GetByIdWithMembersAsync(
            WatchSpaceId.From(query.WatchSpaceId), cancellationToken)
            ?? throw new WatchSpaceNotFoundException(query.WatchSpaceId);

        if (!watchSpace.Members.Any(m => m.UserId == query.RequestingUserId))
            throw new NotAMemberException();

        var members = watchSpace.Members
            .Select(m => new MemberDetail(m.UserId, m.Role.ToString(), m.JoinedAtUtc))
            .ToList();

        return new WatchSpaceDetail(watchSpace.Id.Value, watchSpace.Name, watchSpace.CreatedAtUtc, members);
    }
}

/// <summary>
/// Thrown when the requesting user is not a member of the watch space they are trying to access.
/// </summary>
public sealed class NotAMemberException()
    : Exception("You are not a member of this watch space.");
