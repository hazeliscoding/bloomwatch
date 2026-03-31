using BloomWatch.Modules.WatchSpaces.Application.Abstractions;
using BloomWatch.Modules.WatchSpaces.Application.UseCases.RenameWatchSpace;
using BloomWatch.Modules.WatchSpaces.Domain.Aggregates;
using BloomWatch.Modules.WatchSpaces.Domain.Repositories;
using BloomWatch.Modules.WatchSpaces.Domain.ValueObjects;
using MediatR;

namespace BloomWatch.Modules.WatchSpaces.Application.UseCases.GetWatchSpaceById;

/// <summary>
/// Handles <see cref="GetWatchSpaceByIdQuery"/> by loading the watch space with its members
/// and verifying the requesting user has access.
/// </summary>
public sealed class GetWatchSpaceByIdQueryHandler(
    IWatchSpaceRepository repository,
    IUserDisplayNameLookup displayNameLookup)
    : IRequestHandler<GetWatchSpaceByIdQuery, WatchSpaceDetail>
{
    /// <summary>
    /// Retrieves a watch space by its identifier, including the full member list with display names.
    /// Only members of the watch space can access its details.
    /// </summary>
    public async Task<WatchSpaceDetail> Handle(
        GetWatchSpaceByIdQuery query,
        CancellationToken cancellationToken)
    {
        var watchSpace = await repository.GetByIdWithMembersAsync(
            WatchSpaceId.From(query.WatchSpaceId), cancellationToken)
            ?? throw new WatchSpaceNotFoundException(query.WatchSpaceId);

        if (!watchSpace.Members.Any(m => m.UserId == query.RequestingUserId))
            throw new NotAMemberException();

        var memberUserIds = watchSpace.Members.Select(m => m.UserId);
        var displayNames = await displayNameLookup.GetDisplayNamesAsync(
            memberUserIds, cancellationToken);

        var members = watchSpace.Members
            .Select(m => new MemberDetail(
                m.UserId,
                displayNames.GetValueOrDefault(m.UserId, "Unknown"),
                m.Role.ToString(),
                m.JoinedAtUtc))
            .ToList();

        return new WatchSpaceDetail(watchSpace.Id.Value, watchSpace.Name, watchSpace.CreatedAtUtc, members);
    }
}

/// <summary>
/// Thrown when the requesting user is not a member of the watch space they are trying to access.
/// </summary>
public sealed class NotAMemberException()
    : Exception("You are not a member of this watch space.");
