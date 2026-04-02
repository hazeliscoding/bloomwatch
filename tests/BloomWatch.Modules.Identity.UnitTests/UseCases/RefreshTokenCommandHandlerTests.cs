using BloomWatch.Modules.Identity.Application.Abstractions;
using BloomWatch.Modules.Identity.Application.UseCases.RefreshToken;
using BloomWatch.Modules.Identity.Domain.Aggregates;
using BloomWatch.Modules.Identity.Domain.Repositories;
using BloomWatch.Modules.Identity.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace BloomWatch.Modules.Identity.UnitTests.UseCases;

public sealed class RefreshTokenCommandHandlerTests
{
    private readonly IRefreshTokenService _tokenService = Substitute.For<IRefreshTokenService>();
    private readonly IRefreshTokenRepository _tokenRepository = Substitute.For<IRefreshTokenRepository>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IJwtTokenGenerator _jwtGenerator = Substitute.For<IJwtTokenGenerator>();
    private readonly RefreshTokenCommandHandler _sut;

    public RefreshTokenCommandHandlerTests()
    {
        _sut = new RefreshTokenCommandHandler(_tokenService, _tokenRepository, _userRepository, _jwtGenerator);
        _tokenService.HashToken(Arg.Any<string>()).Returns(ci => $"hash-of-{ci.Arg<string>()}");
        _tokenService.GenerateToken().Returns("new-refresh-token");
    }

    private static User MakeUser() =>
        User.Register(EmailAddress.From("user@example.com"), "hash", DisplayName.From("Alice"));

    private static RefreshToken MakeValidToken(UserId userId) =>
        RefreshToken.Create(userId, "hash-of-old-token", DateTime.UtcNow.AddDays(30));

    [Fact]
    public async Task Handle_ValidToken_ReturnsNewTokenPair()
    {
        var user = MakeUser();
        var storedToken = MakeValidToken(user.Id);
        _tokenRepository.GetByHashAsync("hash-of-old-token").Returns(storedToken);
        _userRepository.GetByIdAsync(user.Id).Returns(user);
        var jwtExpiry = DateTime.UtcNow.AddHours(1);
        _jwtGenerator.GenerateToken(user).Returns(new TokenResult("new-jwt", jwtExpiry));

        var result = await _sut.Handle(new RefreshTokenCommand("old-token"), CancellationToken.None);

        result.AccessToken.Should().Be("new-jwt");
        result.ExpiresAt.Should().Be(jwtExpiry);
        result.RefreshToken.Should().Be("new-refresh-token");
        result.RefreshTokenExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(30), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_ValidToken_RevokesOldToken()
    {
        var user = MakeUser();
        var storedToken = MakeValidToken(user.Id);
        _tokenRepository.GetByHashAsync("hash-of-old-token").Returns(storedToken);
        _userRepository.GetByIdAsync(user.Id).Returns(user);
        _jwtGenerator.GenerateToken(user).Returns(new TokenResult("jwt", DateTime.UtcNow.AddHours(1)));

        await _sut.Handle(new RefreshTokenCommand("old-token"), CancellationToken.None);

        await _tokenRepository.Received(1).RevokeAsync(storedToken, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UnknownToken_ThrowsInvalidRefreshTokenException()
    {
        _tokenRepository.GetByHashAsync(Arg.Any<string>()).Returns((RefreshToken?)null);

        var act = async () => await _sut.Handle(new RefreshTokenCommand("unknown"), CancellationToken.None);
        await act.Should().ThrowAsync<InvalidRefreshTokenException>();
    }

    [Fact]
    public async Task Handle_ExpiredToken_ThrowsInvalidRefreshTokenException()
    {
        var userId = UserId.New();
        var expiredToken = RefreshToken.Create(userId, "hash-of-expired", DateTime.UtcNow.AddSeconds(-1));
        _tokenRepository.GetByHashAsync("hash-of-expired").Returns(expiredToken);

        var act = async () => await _sut.Handle(new RefreshTokenCommand("expired"), CancellationToken.None);
        await act.Should().ThrowAsync<InvalidRefreshTokenException>();
    }

    [Fact]
    public async Task Handle_RevokedToken_ThrowsInvalidRefreshTokenException()
    {
        var userId = UserId.New();
        var revokedToken = RefreshToken.Create(userId, "hash-of-revoked", DateTime.UtcNow.AddDays(30));
        revokedToken.Revoke();
        _tokenRepository.GetByHashAsync("hash-of-revoked").Returns(revokedToken);

        var act = async () => await _sut.Handle(new RefreshTokenCommand("revoked"), CancellationToken.None);
        await act.Should().ThrowAsync<InvalidRefreshTokenException>();
    }
}
