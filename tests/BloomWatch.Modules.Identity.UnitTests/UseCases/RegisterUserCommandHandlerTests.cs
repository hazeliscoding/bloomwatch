using BloomWatch.Modules.Identity.Application.Abstractions;
using BloomWatch.Modules.Identity.Application.UseCases.Register;
using BloomWatch.Modules.Identity.Domain.Aggregates;
using BloomWatch.Modules.Identity.Domain.Repositories;
using BloomWatch.Modules.Identity.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace BloomWatch.Modules.Identity.UnitTests.UseCases;

public sealed class RegisterUserCommandHandlerTests
{
    private readonly IUserRepository _repository = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();
    private readonly RegisterUserCommandHandler _sut;

    public RegisterUserCommandHandlerTests()
    {
        _sut = new RegisterUserCommandHandler(_repository, _hasher);
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_CreatesUserAndReturnsResult()
    {
        _repository.ExistsWithEmailAsync(Arg.Any<EmailAddress>()).Returns(false);
        _hasher.Hash(Arg.Any<string>()).Returns("hashed-password");

        var command = new RegisterUserCommand("user@example.com", "password123", "Alice");
        var result = await _sut.Handle(command, CancellationToken.None);

        result.UserId.Should().NotBeEmpty();
        result.Email.Should().Be("user@example.com");
        result.DisplayName.Should().Be("Alice");
        await _repository.Received(1).AddAsync(Arg.Any<User>());
    }

    [Fact]
    public async Task HandleAsync_DuplicateEmail_ThrowsDuplicateEmailException()
    {
        _repository.ExistsWithEmailAsync(Arg.Any<EmailAddress>()).Returns(true);

        var command = new RegisterUserCommand("taken@example.com", "password123", "Bob");
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DuplicateEmailException>();
        await _repository.DidNotReceive().AddAsync(Arg.Any<User>());
    }

    [Fact]
    public async Task HandleAsync_ShortPassword_ThrowsRegistrationException()
    {
        _repository.ExistsWithEmailAsync(Arg.Any<EmailAddress>()).Returns(false);

        var command = new RegisterUserCommand("user@example.com", "short", "Alice");
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<RegistrationException>();
    }
}
