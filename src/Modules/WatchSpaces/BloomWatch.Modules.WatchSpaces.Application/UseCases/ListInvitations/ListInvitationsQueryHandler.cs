using BloomWatch.Modules.WatchSpaces.Application.UseCases.RenameWatchSpace;
using BloomWatch.Modules.WatchSpaces.Domain.Aggregates;
using BloomWatch.Modules.WatchSpaces.Domain.Enums;
using BloomWatch.Modules.WatchSpaces.Domain.Repositories;
using BloomWatch.Modules.WatchSpaces.Domain.ValueObjects;

namespace BloomWatch.Modules.WatchSpaces.Application.UseCases.ListInvitations;

/// <summary>
/// Handles <see cref="ListInvitationsQuery"/> by loading the watch space aggregate,
/// verifying the requesting user is the owner, and projecting all invitations.
/// </summary>
/// <param name="repository">The watch space repository used for querying.</param>
public sealed class ListInvitationsQueryHandler(IWatchSpaceRepository repository)
{
    /// <summary>
    /// Lists all invitations for a watch space. Access is restricted to the watch space owner.
    /// </summary>
    /// <param name="query">The query containing the watch space identifier and requesting user.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A read-only list of <see cref="InvitationDetail"/> projections for all invitations in the watch space.</returns>
    /// <exception cref="WatchSpaceNotFoundException">Thrown when no watch space exists with the given identifier.</exception>
    /// <exception cref="NotAnOwnerException">Thrown when the requesting user is not the owner of the watch space.</exception>
    public async Task<IReadOnlyList<InvitationDetail>> HandleAsync(
        ListInvitationsQuery query,
        CancellationToken cancellationToken = default)
    {
        var watchSpace = await repository.GetByIdWithMembersAsync(
            WatchSpaceId.From(query.WatchSpaceId), cancellationToken)
            ?? throw new WatchSpaceNotFoundException(query.WatchSpaceId);

        var requestingMember = watchSpace.Members.FirstOrDefault(m => m.UserId == query.RequestingUserId);
        if (requestingMember is null || requestingMember.Role != WatchSpaceRole.Owner)
            throw new NotAnOwnerException();

        return watchSpace.Invitations
            .Select(i => new InvitationDetail(i.Id, i.InvitedEmail, i.Status.ToString(), i.ExpiresAtUtc, i.CreatedAtUtc))
            .ToList();
    }
}
