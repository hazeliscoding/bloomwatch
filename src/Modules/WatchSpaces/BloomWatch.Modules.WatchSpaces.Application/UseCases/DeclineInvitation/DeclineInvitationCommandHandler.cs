using BloomWatch.Modules.WatchSpaces.Domain.Aggregates;
using BloomWatch.Modules.WatchSpaces.Domain.Repositories;

namespace BloomWatch.Modules.WatchSpaces.Application.UseCases.DeclineInvitation;

public sealed class DeclineInvitationCommandHandler(IWatchSpaceRepository repository)
{
    public async Task HandleAsync(
        DeclineInvitationCommand command,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        var watchSpace = await repository.GetByInvitationTokenAsync(command.Token, cancellationToken)
            ?? throw new InvitationNotFoundException();

        watchSpace.DeclineInvitation(command.Token, command.DecliningUserEmail, now);
        await repository.SaveChangesAsync(cancellationToken);
    }
}
