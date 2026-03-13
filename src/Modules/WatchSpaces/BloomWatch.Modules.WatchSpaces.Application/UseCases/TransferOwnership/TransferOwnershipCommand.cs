namespace BloomWatch.Modules.WatchSpaces.Application.UseCases.TransferOwnership;

public sealed record TransferOwnershipCommand(Guid WatchSpaceId, Guid NewOwnerId, Guid RequestingUserId);
