using BloomWatch.Modules.Identity.Application.Abstractions;
using BloomWatch.Modules.Identity.Domain.Aggregates;
using BloomWatch.Modules.Identity.Domain.Repositories;
using BloomWatch.Modules.Identity.Domain.ValueObjects;
using MediatR;

namespace BloomWatch.Modules.Identity.Application.UseCases.Login;

/// <summary>
/// Handles user authentication by validating credentials, checking account status,
/// and issuing a JWT access token upon successful login.
/// </summary>
public sealed class LoginUserCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator,
    IRefreshTokenService refreshTokenService,
    IRefreshTokenRepository refreshTokenRepository)
    : IRequestHandler<LoginUserCommand, LoginUserResult>
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator = jwtTokenGenerator;
    private readonly IRefreshTokenService _refreshTokenService = refreshTokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository = refreshTokenRepository;

    /// <summary>
    /// Processes a login command by authenticating the user and returning an access token.
    /// </summary>
    /// <remarks>
    /// This method performs the following steps:
    /// <list type="number">
    ///   <item>Parses and validates the email address format.</item>
    ///   <item>Looks up the user by email address.</item>
    ///   <item>Verifies that the account is in an <see cref="AccountStatus.Active"/> state.</item>
    ///   <item>Verifies the plain-text password against the stored hash.</item>
    ///   <item>Generates and returns a signed JWT access token.</item>
    /// </list>
    /// For security, invalid email format, unknown email, and wrong password all raise
    /// the same <see cref="InvalidCredentialsException"/> to prevent user enumeration.
    /// </remarks>
    /// <param name="command">The login command containing the user's email and password.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="LoginUserResult"/> containing the access token and its expiration time.</returns>
    /// <exception cref="InvalidCredentialsException">
    /// Thrown when the email format is invalid, no account matches the email, or the password is incorrect.
    /// </exception>
    /// <exception cref="AccountNotActiveException">Thrown when the matched account is not in an active state.</exception>
    public async Task<LoginUserResult> Handle(
        LoginUserCommand command,
        CancellationToken cancellationToken)
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

        var plainRefreshToken = _refreshTokenService.GenerateToken();
        var refreshHash = _refreshTokenService.HashToken(plainRefreshToken);
        var refreshExpiresAt = DateTime.UtcNow.AddDays(30);
        var refreshToken = Domain.Aggregates.RefreshToken.Create(user.Id, refreshHash, refreshExpiresAt);
        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);

        return new LoginUserResult(token.AccessToken, token.ExpiresAt, plainRefreshToken, refreshExpiresAt);
    }
}

/// <summary>
/// Thrown when a login attempt fails due to an invalid email or incorrect password.
/// </summary>
/// <remarks>
/// This exception is intentionally vague to prevent user enumeration attacks.
/// It does not distinguish between an unknown email and a wrong password.
/// </remarks>
public sealed class InvalidCredentialsException()
    : Exception("Invalid email or password.");

/// <summary>
/// Thrown when a login attempt targets an account that is not in an active state
/// (for example, suspended or pending verification).
/// </summary>
public sealed class AccountNotActiveException()
    : Exception("Account is not active.");
