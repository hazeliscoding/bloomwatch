using BloomWatch.Modules.Identity.Application.Abstractions;
using BloomWatch.Modules.Identity.Application.UseCases.ForgotPassword;
using BloomWatch.Modules.Identity.Domain.Aggregates;
using BloomWatch.Modules.Identity.Domain.Repositories;
using BloomWatch.Modules.Identity.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace BloomWatch.Modules.Identity.UnitTests.UseCases;

public sealed class ForgotPasswordCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordResetTokenRepository _tokenRepository = Substitute.For<IPasswordResetTokenRepository>();
    private readonly IRefreshTokenService _tokenService = Substitute.For<IRefreshTokenService>();
    private readonly IPasswordResetEmailSender _emailSender = Substitute.For<IPasswordResetEmailSender>();
    private readonly ForgotPasswordCommandHandler _sut;

    public ForgotPasswordCommandHandlerTests()
    {
        _sut = new ForgotPasswordCommandHandler(
            _userRepository, _tokenRepository, _tokenService, _emailSender);
        _tokenService.GenerateToken().Returns("plain-token");
        _tokenService.HashToken("plain-token").Returns("hash-of-plain-token");
    }

    private static User MakeActiveUser()
        => User.Register(
            EmailAddress.From("user@example.com"),
            "hashed-pw",
            DisplayName.From("Alice"));

    [Fact]
    public async Task Handle_UnknownEmail_ReturnsSilentlyWithoutSendingEmail()
    {
        _userRepository.GetByEmailAsync(Arg.Any<EmailAddress>()).Returns((User?)null);

        await _sut.Handle(new ForgotPasswordCommand("unknown@example.com"), CancellationToken.None);

        await _tokenRepository.DidNotReceive().AddAsync(Arg.Any<PasswordResetToken>());
        await _emailSender.DidNotReceive().SendAsync(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_InvalidEmailFormat_ReturnsSilentlyWithoutSendingEmail()
    {
        await _sut.Handle(new ForgotPasswordCommand("not-an-email"), CancellationToken.None);

        await _userRepository.DidNotReceive().GetByEmailAsync(Arg.Any<EmailAddress>());
        await _emailSender.DidNotReceive().SendAsync(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_ActiveUser_CreatesTokenAndSendsEmail()
    {
        var user = MakeActiveUser();
        _userRepository.GetByEmailAsync(Arg.Any<EmailAddress>()).Returns(user);

        await _sut.Handle(new ForgotPasswordCommand("user@example.com"), CancellationToken.None);

        await _tokenRepository.Received(1).AddAsync(
            Arg.Is<PasswordResetToken>(t => t.TokenHash == "hash-of-plain-token" && !t.IsUsed),
            Arg.Any<CancellationToken>());
        await _emailSender.Received(1).SendAsync("user@example.com", "plain-token", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ActiveUser_SavesChangesAfterAdd()
    {
        var user = MakeActiveUser();
        _userRepository.GetByEmailAsync(Arg.Any<EmailAddress>()).Returns(user);

        await _sut.Handle(new ForgotPasswordCommand("user@example.com"), CancellationToken.None);

        await _tokenRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
