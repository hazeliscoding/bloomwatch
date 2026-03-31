using BloomWatch.Modules.Identity.Application.UseCases.GetProfile;
using BloomWatch.Modules.Identity.Domain.Aggregates;
using BloomWatch.Modules.Identity.Domain.Repositories;
using BloomWatch.Modules.Identity.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace BloomWatch.Modules.Identity.UnitTests.UseCases;

public sealed class GetUserProfileQueryHandlerTests
{
    private readonly IUserRepository _repository = Substitute.For<IUserRepository>();
    private readonly GetUserProfileQueryHandler _sut;

    public GetUserProfileQueryHandlerTests()
    {
        _sut = new GetUserProfileQueryHandler(_repository);
    }

    private static User MakeUser(string email = "alice@example.com", string displayName = "Alice")
        => User.Register(EmailAddress.From(email), "hashed-password", DisplayName.From(displayName));

    [Fact]
    public async Task HandleAsync_ExistingUser_ReturnsMappedProfile()
    {
        var user = MakeUser();
        _repository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        var result = await _sut.Handle(new GetUserProfileQuery(user.Id), CancellationToken.None);

        result.UserId.Should().Be(user.Id.Value);
        result.Email.Should().Be("alice@example.com");
        result.DisplayName.Should().Be("Alice");
        result.AccountStatus.Should().Be("Active");
        result.IsEmailVerified.Should().BeFalse();
        result.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task HandleAsync_UserNotFound_ThrowsUserNotFoundException()
    {
        var missingId = UserId.New();
        _repository.GetByIdAsync(missingId, Arg.Any<CancellationToken>()).Returns((User?)null);

        var act = async () => await _sut.Handle(new GetUserProfileQuery(missingId), CancellationToken.None);

        await act.Should().ThrowAsync<UserNotFoundException>();
    }
}
