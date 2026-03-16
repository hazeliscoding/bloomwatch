using BloomWatch.Modules.Identity.Domain.Repositories;

namespace BloomWatch.Modules.Identity.Application.UseCases.GetProfile;

/// <summary>
/// Handles user profile retrieval by looking up the user aggregate and projecting it
/// into a read-only result.
/// </summary>
public sealed class GetUserProfileQueryHandler
{
    private readonly IUserRepository _userRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetUserProfileQueryHandler"/> class.
    /// </summary>
    /// <param name="userRepository">The repository for querying user aggregates by ID.</param>
    public GetUserProfileQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <summary>
    /// Processes a profile query by retrieving the user and mapping their data to a result.
    /// </summary>
    /// <remarks>
    /// This method looks up the user by their strongly-typed <see cref="Domain.ValueObjects.UserId"/>
    /// and projects the aggregate into a flat <see cref="UserProfileResult"/> DTO.
    /// If no user is found, a <see cref="UserNotFoundException"/> is thrown rather than returning null.
    /// </remarks>
    /// <param name="query">The query containing the user ID to look up.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="UserProfileResult"/> containing the user's profile data.</returns>
    /// <exception cref="UserNotFoundException">Thrown when no user exists with the specified ID.</exception>
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

/// <summary>
/// Thrown when a user profile lookup fails because no user exists with the given ID.
/// </summary>
/// <param name="userId">The ID that was not found.</param>
public sealed class UserNotFoundException(Guid userId)
    : Exception($"User with ID '{userId}' was not found.");
