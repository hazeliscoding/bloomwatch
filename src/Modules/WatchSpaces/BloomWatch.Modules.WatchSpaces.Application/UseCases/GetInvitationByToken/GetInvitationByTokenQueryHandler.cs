using BloomWatch.Modules.WatchSpaces.Domain.Aggregates;
using BloomWatch.Modules.WatchSpaces.Domain.Repositories;

namespace BloomWatch.Modules.WatchSpaces.Application.UseCases.GetInvitationByToken;

/// <summary>
/// Handles <see cref="GetInvitationByTokenQuery"/> by resolving the watch space from the
/// invitation token and returning a preview of the invitation for the invitee.
/// </summary>
/// <param name="repository">The watch space repository.</param>
public sealed class GetInvitationByTokenQueryHandler(IWatchSpaceRepository repository)
{
    /// <summary>
    /// Retrieves the invitation preview for the given token.
    /// </summary>
    /// <param name="query">The query containing the token and requesting user's email.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The invitation preview result.</returns>
    /// <exception cref="InvitationNotFoundException">Thrown when no invitation matches the token.</exception>
    public async Task<InvitationPreviewResult> HandleAsync(
        GetInvitationByTokenQuery query,
        CancellationToken cancellationToken = default)
    {
        var watchSpace = await repository.GetByInvitationTokenAsync(query.Token, cancellationToken)
            ?? throw new InvitationNotFoundException();

        var invitation = watchSpace.Invitations.FirstOrDefault(i => i.Token == query.Token)
            ?? throw new InvitationNotFoundException();

        return new InvitationPreviewResult(
            watchSpace.Id.Value,
            watchSpace.Name,
            invitation.InvitedEmail,
            invitation.Status.ToString(),
            invitation.ExpiresAtUtc);
    }
}
