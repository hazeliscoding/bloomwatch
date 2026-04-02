using BloomWatch.Modules.Identity.Application.Abstractions;
using BloomWatch.Modules.Identity.Application.UseCases.RevokeToken;
using BloomWatch.Modules.Identity.Domain.Aggregates;
using BloomWatch.Modules.Identity.Domain.Repositories;
using BloomWatch.Modules.Identity.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace BloomWatch.Modules.Identity.UnitTests.UseCases;

public sealed class RevokeTokenCommandHandlerTests
{
    private readonly IRefreshTokenService _tokenService = Substitute.For<IRefreshTokenService>();
    private readonly IRefreshTokenRepository _tokenRepository = Substitute.For<IRefreshTokenRepository>();
    private readonly RevokeTokenCommandHandler _sut;

    public RevokeTokenCommandHandlerTests()
    {
        _sut = new RevokeTokenCommandHandler(_tokenService, _tokenRepository);
        _tokenService.HashToken(Arg.Any<string>()).Returns(ci => $"hash-of-{ci.Arg<string>()}");
    }

    [Fact]
    public async Task Handle_ValidToken_RevokesIt()
    {
        var token = RefreshToken.Create(UserId.New(), "hash-of-my-token", DateTime.UtcNow.AddDays(30));
        _tokenRepository.GetByHashAsync("hash-of-my-token").Returns(token);

        await _sut.Handle(new RevokeTokenCommand("my-token"), CancellationToken.None);

        await _tokenRepository.Received(1).RevokeAsync(token, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UnknownToken_DoesNotThrow()
    {
        _tokenRepository.GetByHashAsync(Arg.Any<string>()).Returns((RefreshToken?)null);

        var act = async () => await _sut.Handle(new RevokeTokenCommand("unknown"), CancellationToken.None);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_AlreadyRevokedToken_DoesNotThrow()
    {
        var token = RefreshToken.Create(UserId.New(), "hash-of-already-revoked", DateTime.UtcNow.AddDays(30));
        token.Revoke();
        _tokenRepository.GetByHashAsync("hash-of-already-revoked").Returns(token);

        var act = async () => await _sut.Handle(new RevokeTokenCommand("already-revoked"), CancellationToken.None);
        await act.Should().NotThrowAsync();
        await _tokenRepository.DidNotReceive().RevokeAsync(Arg.Any<RefreshToken>(), Arg.Any<CancellationToken>());
    }
}
