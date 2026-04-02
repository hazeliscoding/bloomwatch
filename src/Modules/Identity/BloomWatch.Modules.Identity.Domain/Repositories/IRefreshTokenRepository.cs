using BloomWatch.Modules.Identity.Domain.Aggregates;

namespace BloomWatch.Modules.Identity.Domain.Repositories;

/// <summary>
/// Persistence contract for <see cref="RefreshToken"/> entities within the Identity module.
/// </summary>
public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken token, CancellationToken cancellationToken = default);
    Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task RevokeAsync(RefreshToken token, CancellationToken cancellationToken = default);
}
