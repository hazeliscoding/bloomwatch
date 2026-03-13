using BloomWatch.Modules.WatchSpaces.Application.UseCases.RenameWatchSpace;
using BloomWatch.Modules.WatchSpaces.Domain.Aggregates;
using BloomWatch.Modules.WatchSpaces.Domain.Repositories;
using BloomWatch.Modules.WatchSpaces.Domain.ValueObjects;

namespace BloomWatch.Modules.WatchSpaces.Application.UseCases.GetWatchSpaceById;

public sealed class GetWatchSpaceByIdQueryHandler(IWatchSpaceRepository repository)
{
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

public sealed class NotAMemberException()
    : Exception("You are not a member of this watch space.");
