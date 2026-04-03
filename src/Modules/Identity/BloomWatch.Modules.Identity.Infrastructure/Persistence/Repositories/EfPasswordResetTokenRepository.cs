using BloomWatch.Modules.Identity.Domain.Aggregates;
using BloomWatch.Modules.Identity.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BloomWatch.Modules.Identity.Infrastructure.Persistence.Repositories;

internal sealed class EfPasswordResetTokenRepository(IdentityDbContext dbContext) : IPasswordResetTokenRepository
{
    public async Task AddAsync(PasswordResetToken token, CancellationToken cancellationToken = default)
        => await dbContext.PasswordResetTokens.AddAsync(token, cancellationToken);

    public Task<PasswordResetToken?> FindByHashAsync(string tokenHash, CancellationToken cancellationToken = default)
        => dbContext.PasswordResetTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => dbContext.SaveChangesAsync(cancellationToken);
}
