using BloomWatch.Modules.WatchSpaces.Application.UseCases.RenameWatchSpace;
using BloomWatch.Modules.WatchSpaces.Domain.Aggregates;
using BloomWatch.Modules.WatchSpaces.Domain.Enums;
using BloomWatch.Modules.WatchSpaces.Domain.Repositories;
using BloomWatch.Modules.WatchSpaces.Domain.ValueObjects;

namespace BloomWatch.Modules.WatchSpaces.Application.UseCases.ListInvitations;

public sealed class ListInvitationsQueryHandler(IWatchSpaceRepository repository)
{
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
