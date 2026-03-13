using BloomWatch.Modules.WatchSpaces.Application.Abstractions;
using BloomWatch.Modules.WatchSpaces.Application.UseCases.RenameWatchSpace;
using BloomWatch.Modules.WatchSpaces.Domain.Repositories;
using BloomWatch.Modules.WatchSpaces.Domain.ValueObjects;

namespace BloomWatch.Modules.WatchSpaces.Application.UseCases.InviteMember;

public sealed class InviteMemberCommandHandler(
    IWatchSpaceRepository repository,
    IUserReadModel userReadModel,
    IInvitationEmailSender emailSender)
{
    private static readonly TimeSpan DefaultExpiry = TimeSpan.FromDays(7);

    public async Task<InviteMemberResult> HandleAsync(
        InviteMemberCommand command,
        CancellationToken cancellationToken = default)
    {
        var watchSpace = await repository.GetByIdWithMembersAsync(
            WatchSpaceId.From(command.WatchSpaceId), cancellationToken)
            ?? throw new WatchSpaceNotFoundException(command.WatchSpaceId);

        var user = await userReadModel.FindUserByEmailAsync(command.InvitedEmail, cancellationToken)
            ?? throw new InvitedUserNotFoundException(command.InvitedEmail);

        // Check if already a member
        if (watchSpace.Members.Any(m => m.UserId == user.UserId))
            throw new AlreadyAMemberException(command.InvitedEmail);

        var expiresAt = DateTime.UtcNow.Add(DefaultExpiry);
        var invitation = watchSpace.InviteMember(command.InvitedEmail, command.RequestingUserId, expiresAt);

        await repository.SaveChangesAsync(cancellationToken);

        await emailSender.SendAsync(
            command.InvitedEmail,
            invitation.Token,
            watchSpace.Name,
            cancellationToken);

        return new InviteMemberResult(
            invitation.Id,
            invitation.InvitedEmail,
            invitation.Status.ToString(),
            invitation.ExpiresAtUtc,
            invitation.Token);
    }
}

public sealed class InvitedUserNotFoundException(string email)
    : Exception($"No registered user found with email '{email}'.");

public sealed class AlreadyAMemberException(string email)
    : Exception($"User with email '{email}' is already a member of this watch space.");
