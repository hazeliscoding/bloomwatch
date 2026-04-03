using BloomWatch.Modules.Identity.Application.Abstractions;
using BloomWatch.Modules.Identity.Application.UseCases.ResetPassword;
using BloomWatch.Modules.Identity.Domain.Aggregates;
using BloomWatch.Modules.Identity.Domain.Repositories;
using BloomWatch.Modules.Identity.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace BloomWatch.Modules.Identity.UnitTests.UseCases;

public sealed class ResetPasswordCommandHandlerTests
{
    private readonly IPasswordResetTokenRepository _tokenRepository =
        Substitute.For<IPasswordResetTokenRepository>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IRefreshTokenService _tokenService = Substitute.For<IRefreshTokenService>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly ResetPasswordCommandHandler _sut;

    private const string ValidPassword = "Password1";
    private const string PlainToken = "my-plain-token";
    private const string TokenHash = "hash-of-token";

    public ResetPasswordCommandHandlerTests()
    {
        _sut = new ResetPasswordCommandHandler(
            _tokenRepository, _userRepository, _tokenService, _passwordHasher);
        _tokenService.HashToken(PlainToken).Returns(TokenHash);
        _passwordHasher.Hash(Arg.Any<string>()).Returns("new-bcrypt-hash");
    }

    private static User MakeUser()
        => User.Register(EmailAddress.From("user@example.com"), "old-hash", DisplayName.From("Alice"));

    private static PasswordResetToken MakeValidToken(UserId userId)
        => PasswordResetToken.Create(userId, TokenHash, DateTime.UtcNow.AddHours(1));

    [Fact]
    public async Task Handle_InvalidToken_ReturnsInvalidToken()
    {
        _tokenRepository.FindByHashAsync(TokenHash).Returns((PasswordResetToken?)null);

        var result = await _sut.Handle(new ResetPasswordCommand(PlainToken, ValidPassword), CancellationToken.None);

        result.Should().Be(ResetPasswordResult.InvalidToken);
    }

    [Fact]
    public async Task Handle_ExpiredToken_ReturnsTokenExpired()
    {
        var userId = UserId.New();
        var expiredToken = PasswordResetToken.Create(userId, TokenHash, DateTime.UtcNow.AddSeconds(-1));
        _tokenRepository.FindByHashAsync(TokenHash).Returns(expiredToken);

        var result = await _sut.Handle(new ResetPasswordCommand(PlainToken, ValidPassword), CancellationToken.None);

        result.Should().Be(ResetPasswordResult.TokenExpired);
    }

    [Fact]
    public async Task Handle_UsedToken_ReturnsTokenAlreadyUsed()
    {
        var userId = UserId.New();
        var usedToken = PasswordResetToken.Create(userId, TokenHash, DateTime.UtcNow.AddHours(1));
        usedToken.MarkUsed();
        _tokenRepository.FindByHashAsync(TokenHash).Returns(usedToken);

        var result = await _sut.Handle(new ResetPasswordCommand(PlainToken, ValidPassword), CancellationToken.None);

        result.Should().Be(ResetPasswordResult.TokenAlreadyUsed);
    }

    [Fact]
    public async Task Handle_WeakPassword_ReturnsWeakPasswordBeforeLookup()
    {
        var result = await _sut.Handle(new ResetPasswordCommand(PlainToken, "weak"), CancellationToken.None);

        result.Should().Be(ResetPasswordResult.WeakPassword);
        await _tokenRepository.DidNotReceive().FindByHashAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_ValidTokenAndPassword_UpdatesPasswordHashAndMarksTokenUsed()
    {
        var user = MakeUser();
        var token = MakeValidToken(user.Id);
        _tokenRepository.FindByHashAsync(TokenHash).Returns(token);
        _userRepository.GetByIdAsync(user.Id).Returns(user);

        var result = await _sut.Handle(new ResetPasswordCommand(PlainToken, ValidPassword), CancellationToken.None);

        result.Should().Be(ResetPasswordResult.Success);
        user.PasswordHash.Should().Be("new-bcrypt-hash");
        token.IsUsed.Should().BeTrue();
        await _tokenRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ValidTokenAndPassword_SavesChangesAtomically()
    {
        var user = MakeUser();
        var token = MakeValidToken(user.Id);
        _tokenRepository.FindByHashAsync(TokenHash).Returns(token);
        _userRepository.GetByIdAsync(user.Id).Returns(user);

        await _sut.Handle(new ResetPasswordCommand(PlainToken, ValidPassword), CancellationToken.None);

        // Both user hash update and token.IsUsed must be committed in a single SaveChanges call
        await _tokenRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        user.PasswordHash.Should().Be("new-bcrypt-hash");
        token.IsUsed.Should().BeTrue();
    }
}
