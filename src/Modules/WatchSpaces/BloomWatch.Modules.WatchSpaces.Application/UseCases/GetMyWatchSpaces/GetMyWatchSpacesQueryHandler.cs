using BloomWatch.Modules.WatchSpaces.Domain.Repositories;

namespace BloomWatch.Modules.WatchSpaces.Application.UseCases.GetMyWatchSpaces;

public sealed class GetMyWatchSpacesQueryHandler(IWatchSpaceRepository repository)
{
    public async Task<IReadOnlyList<WatchSpaceSummary>> HandleAsync(
        GetMyWatchSpacesQuery query,
        CancellationToken cancellationToken = default)
    {
        var spaces = await repository.GetByMemberUserIdAsync(query.UserId, cancellationToken);

        return spaces
            .Select(ws =>
            {
                var member = ws.Members.First(m => m.UserId == query.UserId);
                return new WatchSpaceSummary(ws.Id.Value, ws.Name, ws.CreatedAtUtc, member.Role.ToString());
            })
            .ToList();
    }
}
