using BloomWatch.Modules.Identity.Domain.Aggregates;
using BloomWatch.Modules.Identity.Domain.ValueObjects;

namespace BloomWatch.Modules.Identity.Domain.Repositories;

/// <summary>
/// Defines the persistence contract for <see cref="User"/> aggregates within the Identity module.
/// Implementations are provided by the infrastructure layer (e.g., EF Core).
/// </summary>
/// <remarks>
/// This repository follows the collection-oriented pattern: the domain layer defines
/// what operations are needed, and the infrastructure layer decides how to fulfill them.
/// All methods accept a <see cref="CancellationToken"/> for cooperative cancellation.
/// </remarks>
public interface IUserRepository
{
    /// <summary>
    /// Retrieves a user by their unique identifier.
    /// </summary>
    /// <param name="id">The <see cref="UserId"/> of the user to retrieve.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>
    /// The <see cref="User"/> if found; otherwise, <c>null</c>.
    /// </returns>
    Task<User?> GetByIdAsync(UserId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a user by their email address.
    /// </summary>
    /// <param name="email">The <see cref="EmailAddress"/> to search for. Comparison is case-insensitive
    /// because <see cref="EmailAddress"/> normalizes to lowercase on creation.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>
    /// The <see cref="User"/> if found; otherwise, <c>null</c>.
    /// </returns>
    Task<User?> GetByEmailAsync(EmailAddress email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists a new <see cref="User"/> aggregate to the data store.
    /// </summary>
    /// <param name="user">The <see cref="User"/> aggregate to add. Must not already exist in the store.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task that completes when the user has been added to the underlying persistence context.
    /// Note that the change may not be durable until the unit of work is committed.</returns>
    Task AddAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a user with the specified email address already exists in the data store.
    /// Use this before calling <see cref="User.Register"/> to enforce email uniqueness at the domain level.
    /// </summary>
    /// <param name="email">The <see cref="EmailAddress"/> to check for existence.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>
    /// <c>true</c> if a user with the given email already exists; otherwise, <c>false</c>.
    /// </returns>
    Task<bool> ExistsWithEmailAsync(EmailAddress email, CancellationToken cancellationToken = default);
}
