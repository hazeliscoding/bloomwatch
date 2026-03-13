using BloomWatch.Modules.Identity.Domain.Aggregates;
using BloomWatch.Modules.Identity.Domain.Repositories;
using BloomWatch.Modules.Identity.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace BloomWatch.Modules.Identity.Infrastructure.Persistence.Repositories;

internal sealed class EfUserRepository(IdentityDbContext dbContext) : IUserRepository
{
    public Task<User?> GetByIdAsync(UserId id, CancellationToken cancellationToken = default)
        => dbContext.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<User?> GetByEmailAsync(EmailAddress email, CancellationToken cancellationToken = default)
        => dbContext.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await dbContext.Users.AddAsync(user, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> ExistsWithEmailAsync(EmailAddress email, CancellationToken cancellationToken = default)
        => dbContext.Users.AnyAsync(u => u.Email == email, cancellationToken);
}
