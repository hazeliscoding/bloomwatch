using BloomWatch.Modules.WatchSpaces.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace BloomWatch.Modules.WatchSpaces.Infrastructure.CrossModule;

/// <summary>
/// Read-only cross-schema query against <c>identity.users</c>, implementing
/// <see cref="IUserReadModel"/> for the WatchSpaces application layer.
/// </summary>
/// <remarks>
/// This is a deliberate read coupling as defined in design.md (Decision D4).
/// The WatchSpaces module queries Identity-owned data to resolve user details
/// during invitation workflows without calling Identity services at runtime.
/// </remarks>
/// <param name="dbContext">
/// The <see cref="IdentityReadDbContext"/> configured for read-only access to the Identity schema.
/// </param>
internal sealed class UserReadModel(IdentityReadDbContext dbContext) : IUserReadModel
{
    /// <summary>
    /// Looks up a user by their email address in the <c>identity.users</c> table.
    /// </summary>
    /// <param name="email">The email address to search for (case-sensitive match).</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>
    /// A tuple containing the user's ID, email, and display name if found;
    /// otherwise <see langword="null"/>.
    /// </returns>
    public async Task<(Guid UserId, string Email, string DisplayName)?> FindUserByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var row = await dbContext.Users
            .Where(u => u.Email == email)
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null) return null;
        return (row.UserId, row.Email, row.DisplayName);
    }
}
