namespace BloomWatch.Modules.Identity.Application.UseCases.Register;

public sealed record RegisterUserCommand(string Email, string Password, string DisplayName);

public sealed record RegisterUserResult(Guid UserId, string Email, string DisplayName);
