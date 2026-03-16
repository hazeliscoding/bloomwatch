namespace BloomWatch.Modules.WatchSpaces.Application.Abstractions;

/// <summary>
/// Provides read-only access to user profile data from the identity or user management module.
/// </summary>
/// <remarks>
/// This is an anti-corruption layer abstraction. The WatchSpaces module does not own user data
/// but needs to resolve user identities (e.g., when inviting members by email).
/// Implementations typically query a shared read model or call another module's public API.
/// </remarks>
public interface IUserReadModel
{
    /// <summary>
    /// Looks up a registered user by their email address.
    /// </summary>
    /// <param name="email">The email address to search for (case-insensitive).</param>
    /// <param name="cancellationToken">A token to cancel the lookup.</param>
    /// <returns>
    /// A tuple containing the user's <c>UserId</c>, <c>Email</c>, and <c>DisplayName</c>
    /// if a matching user exists; otherwise, <c>null</c>.
    /// </returns>
    Task<(Guid UserId, string Email, string DisplayName)?> FindUserByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);
}
