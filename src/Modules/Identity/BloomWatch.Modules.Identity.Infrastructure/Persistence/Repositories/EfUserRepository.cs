using BloomWatch.Modules.Identity.Domain.Aggregates;
using BloomWatch.Modules.Identity.Domain.Repositories;
using BloomWatch.Modules.Identity.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace BloomWatch.Modules.Identity.Infrastructure.Persistence.Repositories;

/// <summary>
/// Entity Framework Core implementation of <see cref="IUserRepository"/> backed by
/// <see cref="IdentityDbContext"/>.
/// </summary>
/// <remarks>
/// All queries target the <c>identity.users</c> table. The <see cref="AddAsync"/> method
/// calls <see cref="DbContext.SaveChangesAsync(CancellationToken)"/> immediately, so each
/// insertion is committed as its own unit of work.
/// </remarks>
/// <param name="dbContext">The Identity module's EF Core context.</param>
internal sealed class EfUserRepository(IdentityDbContext dbContext) : IUserRepository
{
    /// <inheritdoc />
    /// <remarks>Performs a single-row lookup on the primary key (<c>user_id</c>).</remarks>
    public Task<User?> GetByIdAsync(UserId id, CancellationToken cancellationToken = default)
        => dbContext.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    /// <inheritdoc />
    /// <remarks>Performs a lookup against the unique <c>ix_users_email</c> index.</remarks>
    public Task<User?> GetByEmailAsync(EmailAddress email, CancellationToken cancellationToken = default)
        => dbContext.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    /// <inheritdoc />
    /// <remarks>
    /// Adds the <paramref name="user"/> to the change tracker and immediately flushes changes
    /// to the database via <see cref="DbContext.SaveChangesAsync(CancellationToken)"/>.
    /// </remarks>
    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await dbContext.Users.AddAsync(user, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    /// <remarks>Translates to an <c>EXISTS</c> SQL query filtered on the <c>email</c> column.</remarks>
    public Task<bool> ExistsWithEmailAsync(EmailAddress email, CancellationToken cancellationToken = default)
        => dbContext.Users.AnyAsync(u => u.Email == email, cancellationToken);
}
