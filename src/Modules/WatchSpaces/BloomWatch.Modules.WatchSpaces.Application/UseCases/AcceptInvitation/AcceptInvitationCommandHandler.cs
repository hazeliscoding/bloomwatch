using BloomWatch.Modules.WatchSpaces.Application.Abstractions;
using BloomWatch.Modules.WatchSpaces.Contracts.IntegrationEvents;
using BloomWatch.Modules.WatchSpaces.Domain.Aggregates;
using BloomWatch.Modules.WatchSpaces.Domain.Repositories;

namespace BloomWatch.Modules.WatchSpaces.Application.UseCases.AcceptInvitation;

public sealed class AcceptInvitationCommandHandler(
    IWatchSpaceRepository repository,
    IIntegrationEventPublisher publisher)
{
    public async Task HandleAsync(
        AcceptInvitationCommand command,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        var watchSpace = await repository.GetByInvitationTokenAsync(command.Token, cancellationToken)
            ?? throw new Domain.Aggregates.InvitationNotFoundException();

        var member = watchSpace.AcceptInvitation(
            command.Token,
            command.AcceptingUserId,
            command.AcceptingUserEmail,
            now);

        await repository.SaveChangesAsync(cancellationToken);

        await publisher.PublishAsync(new MemberJoinedWatchSpace(
            watchSpace.Id.Value,
            member.UserId,
            member.Role.ToString(),
            member.JoinedAtUtc), cancellationToken);
    }
}
