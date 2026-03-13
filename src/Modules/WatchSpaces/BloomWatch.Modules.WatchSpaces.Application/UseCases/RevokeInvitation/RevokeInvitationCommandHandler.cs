using BloomWatch.Modules.WatchSpaces.Application.UseCases.RenameWatchSpace;
using BloomWatch.Modules.WatchSpaces.Domain.Repositories;
using BloomWatch.Modules.WatchSpaces.Domain.ValueObjects;

namespace BloomWatch.Modules.WatchSpaces.Application.UseCases.RevokeInvitation;

public sealed class RevokeInvitationCommandHandler(IWatchSpaceRepository repository)
{
    public async Task HandleAsync(
        RevokeInvitationCommand command,
        CancellationToken cancellationToken = default)
    {
        var watchSpace = await repository.GetByIdWithMembersAsync(
            WatchSpaceId.From(command.WatchSpaceId), cancellationToken)
            ?? throw new WatchSpaceNotFoundException(command.WatchSpaceId);

        watchSpace.RevokeInvitation(command.InvitationId, command.RequestingUserId);
        await repository.SaveChangesAsync(cancellationToken);
    }
}
