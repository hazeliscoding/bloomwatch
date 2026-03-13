namespace BloomWatch.Modules.Identity.Application.UseCases.Login;

public sealed record LoginUserCommand(string Email, string Password);

public sealed record LoginUserResult(string AccessToken, DateTime ExpiresAt);
