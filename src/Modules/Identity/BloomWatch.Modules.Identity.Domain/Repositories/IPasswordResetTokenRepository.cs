using BloomWatch.Modules.Identity.Domain.Aggregates;

namespace BloomWatch.Modules.Identity.Domain.Repositories;

/// <summary>
/// Persistence contract for <see cref="PasswordResetToken"/> entities within the Identity module.
/// </summary>
public interface IPasswordResetTokenRepository
{
    Task AddAsync(PasswordResetToken token, CancellationToken cancellationToken = default);
    Task<PasswordResetToken?> FindByHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
