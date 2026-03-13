namespace BloomWatch.Modules.WatchSpaces.Application.Abstractions;

public interface IUserReadModel
{
    Task<(Guid UserId, string Email, string DisplayName)?> FindUserByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);
}
