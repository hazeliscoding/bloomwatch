using BloomWatch.Modules.Identity.Domain.Aggregates;

namespace BloomWatch.Modules.Identity.Application.Abstractions;

public sealed record TokenResult(string AccessToken, DateTime ExpiresAt);

public interface IJwtTokenGenerator
{
    TokenResult GenerateToken(User user);
}
