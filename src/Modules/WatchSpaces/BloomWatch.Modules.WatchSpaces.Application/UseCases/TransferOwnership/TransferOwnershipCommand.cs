namespace BloomWatch.Modules.WatchSpaces.Application.UseCases.TransferOwnership;

/// <summary>
/// Command to transfer ownership of a watch space from the current owner to another member.
/// </summary>
/// <param name="WatchSpaceId">The unique identifier of the watch space whose ownership is being transferred.</param>
/// <param name="NewOwnerId">The user identifier of the member who will become the new owner.</param>
/// <param name="RequestingUserId">The identifier of the current owner requesting the transfer.</param>
public sealed record TransferOwnershipCommand(Guid WatchSpaceId, Guid NewOwnerId, Guid RequestingUserId);
