using BloomWatch.Modules.Identity.Domain.Aggregates;
using BloomWatch.Modules.Identity.Domain.ValueObjects;

namespace BloomWatch.Modules.Identity.Domain.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(UserId id, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(EmailAddress email, CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    Task<bool> ExistsWithEmailAsync(EmailAddress email, CancellationToken cancellationToken = default);
}
