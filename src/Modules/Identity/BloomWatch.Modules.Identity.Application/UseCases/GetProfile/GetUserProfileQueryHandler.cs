using BloomWatch.Modules.Identity.Domain.Repositories;

namespace BloomWatch.Modules.Identity.Application.UseCases.GetProfile;

public sealed class GetUserProfileQueryHandler
{
    private readonly IUserRepository _userRepository;

    public GetUserProfileQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserProfileResult> HandleAsync(
        GetUserProfileQuery query,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(query.UserId, cancellationToken);

        if (user is null)
            throw new UserNotFoundException(query.UserId.Value);

        return new UserProfileResult(
            user.Id.Value,
            user.Email.Value,
            user.DisplayName.Value,
            user.AccountStatus.ToString(),
            user.IsEmailVerified,
            user.CreatedAtUtc);
    }
}

public sealed class UserNotFoundException(Guid userId)
    : Exception($"User with ID '{userId}' was not found.");
