using BloomWatch.Modules.WatchSpaces.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace BloomWatch.Modules.WatchSpaces.Infrastructure.CrossModule;

/// <summary>
/// Read-only cross-schema query against identity.users.
/// This is a deliberate read coupling as defined in design.md (Decision D4).
/// </summary>
internal sealed class UserReadModel(IdentityReadDbContext dbContext) : IUserReadModel
{
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
