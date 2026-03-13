using BloomWatch.Modules.WatchSpaces.Domain.Aggregates;
using BloomWatch.Modules.WatchSpaces.Domain.Repositories;

namespace BloomWatch.Modules.WatchSpaces.Application.UseCases.CreateWatchSpace;

public sealed class CreateWatchSpaceCommandHandler(IWatchSpaceRepository repository)
{
    public async Task<CreateWatchSpaceResult> HandleAsync(
        CreateWatchSpaceCommand command,
        CancellationToken cancellationToken = default)
    {
        var watchSpace = WatchSpace.Create(command.Name, command.RequestingUserId);

        await repository.AddAsync(watchSpace, cancellationToken);

        return new CreateWatchSpaceResult(
            watchSpace.Id.Value,
            watchSpace.Name,
            watchSpace.CreatedAtUtc,
            "Owner");
    }
}
