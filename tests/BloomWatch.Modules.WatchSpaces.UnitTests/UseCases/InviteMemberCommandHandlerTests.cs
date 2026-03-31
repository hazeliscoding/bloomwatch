using BloomWatch.Modules.WatchSpaces.Application.Abstractions;
using BloomWatch.Modules.WatchSpaces.Application.UseCases.InviteMember;
using BloomWatch.Modules.WatchSpaces.Domain.Aggregates;
using BloomWatch.Modules.WatchSpaces.Domain.Repositories;
using BloomWatch.Modules.WatchSpaces.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace BloomWatch.Modules.WatchSpaces.UnitTests.UseCases;

public sealed class InviteMemberCommandHandlerTests
{
    private readonly IWatchSpaceRepository _repository = Substitute.For<IWatchSpaceRepository>();
    private readonly IUserReadModel _userReadModel = Substitute.For<IUserReadModel>();
    private readonly IUserDisplayNameLookup _displayNameLookup = Substitute.For<IUserDisplayNameLookup>();
    private readonly IInvitationEmailSender _emailSender = Substitute.For<IInvitationEmailSender>();

    private readonly InviteMemberCommandHandler _sut;

    private static readonly Guid SpaceId = Guid.NewGuid();
    private static readonly Guid OwnerId = Guid.NewGuid();
    private static readonly Guid InviteeId = Guid.NewGuid();
    private const string InviteeEmail = "invitee@example.com";

    public InviteMemberCommandHandlerTests()
    {
        _sut = new InviteMemberCommandHandler(
            _repository,
            _userReadModel,
            _displayNameLookup,
            _emailSender,
            NullLogger<InviteMemberCommandHandler>.Instance);
    }

    private WatchSpace BuildOwnerSpace()
    {
        var space = WatchSpace.Create("Anime Club", OwnerId);
        // Reflect the Id so the repository mock can match it
        typeof(WatchSpace)
            .GetProperty(nameof(WatchSpace.Id))!
            .SetValue(space, WatchSpaceId.From(SpaceId));
        return space;
    }

    [Fact]
    public async Task Handle_EmailSentSuccessfully_ReturnsEmailDeliveryFailedFalse()
    {
        var space = BuildOwnerSpace();
        _repository.GetByIdWithMembersAsync(WatchSpaceId.From(SpaceId), default)
            .Returns(space);
        _userReadModel.FindUserByEmailAsync(InviteeEmail, default)
            .Returns((InviteeId, InviteeEmail, "Invitee"));
        _displayNameLookup.GetDisplayNamesAsync(Arg.Any<IEnumerable<Guid>>(), default)
            .Returns(new Dictionary<Guid, string> { [OwnerId] = "Hazel" });

        var result = await _sut.Handle(new InviteMemberCommand(SpaceId, InviteeEmail, OwnerId), default);

        result.EmailDeliveryFailed.Should().BeFalse();
        await _emailSender.Received(1).SendAsync(
            InviteeEmail,
            Arg.Any<string>(),
            "Anime Club",
            "Hazel",
            default);
    }

    [Fact]
    public async Task Handle_EmailSenderThrows_ReturnsEmailDeliveryFailedTrue()
    {
        var space = BuildOwnerSpace();
        _repository.GetByIdWithMembersAsync(WatchSpaceId.From(SpaceId), default)
            .Returns(space);
        _userReadModel.FindUserByEmailAsync(InviteeEmail, default)
            .Returns((InviteeId, InviteeEmail, "Invitee"));
        _displayNameLookup.GetDisplayNamesAsync(Arg.Any<IEnumerable<Guid>>(), default)
            .Returns(new Dictionary<Guid, string> { [OwnerId] = "Hazel" });
        _emailSender
            .SendAsync(default!, default!, default!, default!, default)
            .ThrowsAsyncForAnyArgs(new InvalidOperationException("SMTP unavailable"));

        var result = await _sut.Handle(new InviteMemberCommand(SpaceId, InviteeEmail, OwnerId), default);

        result.EmailDeliveryFailed.Should().BeTrue();
        result.InvitedEmail.Should().Be(InviteeEmail);
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsAndDoesNotCallEmailSender()
    {
        var space = BuildOwnerSpace();
        _repository.GetByIdWithMembersAsync(WatchSpaceId.From(SpaceId), default)
            .Returns(space);
        _userReadModel.FindUserByEmailAsync(InviteeEmail, default)
            .Returns((ValueTuple<Guid, string, string>?)null);

        var act = () => _sut.Handle(new InviteMemberCommand(SpaceId, InviteeEmail, OwnerId), default);

        await act.Should().ThrowAsync<InvitedUserNotFoundException>();
        await _emailSender.DidNotReceiveWithAnyArgs()
            .SendAsync(default!, default!, default!, default!);
    }
}
