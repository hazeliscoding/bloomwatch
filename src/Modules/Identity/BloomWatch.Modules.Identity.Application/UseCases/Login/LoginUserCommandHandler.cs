using BloomWatch.Modules.Identity.Application.Abstractions;
using BloomWatch.Modules.Identity.Domain.Aggregates;
using BloomWatch.Modules.Identity.Domain.Repositories;
using BloomWatch.Modules.Identity.Domain.ValueObjects;

namespace BloomWatch.Modules.Identity.Application.UseCases.Login;

public sealed class LoginUserCommandHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginUserCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<LoginUserResult> HandleAsync(
        LoginUserCommand command,
        CancellationToken cancellationToken = default)
    {
        EmailAddress email;
        try
        {
            email = EmailAddress.From(command.Email);
        }
        catch
        {
            throw new InvalidCredentialsException();
        }

        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);

        if (user is null)
            throw new InvalidCredentialsException();

        if (user.AccountStatus != AccountStatus.Active)
            throw new AccountNotActiveException();

        if (!_passwordHasher.Verify(command.Password, user.PasswordHash))
            throw new InvalidCredentialsException();

        var token = _jwtTokenGenerator.GenerateToken(user);

        return new LoginUserResult(token.AccessToken, token.ExpiresAt);
    }
}

public sealed class InvalidCredentialsException()
    : Exception("Invalid email or password.");

public sealed class AccountNotActiveException()
    : Exception("Account is not active.");
