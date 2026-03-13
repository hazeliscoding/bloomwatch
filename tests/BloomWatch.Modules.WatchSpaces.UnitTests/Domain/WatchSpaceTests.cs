using BloomWatch.Modules.WatchSpaces.Domain.Aggregates;
using BloomWatch.Modules.WatchSpaces.Domain.Enums;
using BloomWatch.Modules.WatchSpaces.Domain.Exceptions;
using FluentAssertions;

namespace BloomWatch.Modules.WatchSpaces.UnitTests.Domain;

public sealed class WatchSpaceTests
{
    private static readonly Guid OwnerUserId = Guid.NewGuid();
    private const string ValidName = "My Anime Space";

    // --- Create ---

    [Fact]
    public void Create_ValidInput_CreatorIsOwner()
    {
        var ws = WatchSpace.Create(ValidName, OwnerUserId);

        ws.Name.Should().Be(ValidName);
        ws.CreatedByUserId.Should().Be(OwnerUserId);
        ws.Members.Should().HaveCount(1);
        ws.Members[0].UserId.Should().Be(OwnerUserId);
        ws.Members[0].Role.Should().Be(WatchSpaceRole.Owner);
    }

    [Fact]
    public void Create_EmptyName_Throws()
    {
        var act = () => WatchSpace.Create("", OwnerUserId);
        act.Should().Throw<WatchSpaceDomainException>();
    }

    [Fact]
    public void Create_NameTooLong_Throws()
    {
        var act = () => WatchSpace.Create(new string('x', 101), OwnerUserId);
        act.Should().Throw<WatchSpaceDomainException>();
    }

    // --- Rename ---

    [Fact]
    public void Rename_ByOwner_UpdatesName()
    {
        var ws = WatchSpace.Create(ValidName, OwnerUserId);
        ws.Rename("New Name", OwnerUserId);
        ws.Name.Should().Be("New Name");
    }

    [Fact]
    public void Rename_ByNonMember_Throws()
    {
        var ws = WatchSpace.Create(ValidName, OwnerUserId);
        var act = () => ws.Rename("New Name", Guid.NewGuid());
        act.Should().Throw<NotAnOwnerException>();
    }

    // --- InviteMember ---

    [Fact]
    public void InviteMember_ByOwner_CreatesInvitation()
    {
        var ws = WatchSpace.Create(ValidName, OwnerUserId);
        var invitation = ws.InviteMember("user@example.com", OwnerUserId, DateTime.UtcNow.AddDays(7));

        invitation.Should().NotBeNull();
        invitation.InvitedEmail.Should().Be("user@example.com");
        invitation.Status.Should().Be(InvitationStatus.Pending);
        ws.Invitations.Should().HaveCount(1);
    }

    [Fact]
    public void InviteMember_DuplicatePending_Throws()
    {
        var ws = WatchSpace.Create(ValidName, OwnerUserId);
        ws.InviteMember("user@example.com", OwnerUserId, DateTime.UtcNow.AddDays(7));

        var act = () => ws.InviteMember("user@example.com", OwnerUserId, DateTime.UtcNow.AddDays(7));
        act.Should().Throw<WatchSpaceDomainException>();
    }

    [Fact]
    public void InviteMember_ByNonOwner_Throws()
    {
        var ws = WatchSpace.Create(ValidName, OwnerUserId);
        var act = () => ws.InviteMember("user@example.com", Guid.NewGuid(), DateTime.UtcNow.AddDays(7));
        act.Should().Throw<NotAnOwnerException>();
    }

    // --- AcceptInvitation ---

    [Fact]
    public void AcceptInvitation_Valid_AddsMember()
    {
        var ws = WatchSpace.Create(ValidName, OwnerUserId);
        var invitation = ws.InviteMember("user@example.com", OwnerUserId, DateTime.UtcNow.AddDays(7));
        var acceptingUserId = Guid.NewGuid();

        var member = ws.AcceptInvitation(invitation.Token, acceptingUserId, "user@example.com", DateTime.UtcNow);

        member.UserId.Should().Be(acceptingUserId);
        member.Role.Should().Be(WatchSpaceRole.Member);
        ws.Members.Should().HaveCount(2);
        invitation.Status.Should().Be(InvitationStatus.Accepted);
    }

    [Fact]
    public void AcceptInvitation_ExpiredToken_Throws()
    {
        var ws = WatchSpace.Create(ValidName, OwnerUserId);
        var invitation = ws.InviteMember("user@example.com", OwnerUserId, DateTime.UtcNow.AddDays(-1));

        var act = () => ws.AcceptInvitation(invitation.Token, Guid.NewGuid(), "user@example.com", DateTime.UtcNow);
        act.Should().Throw<InvitationExpiredException>();
    }

    [Fact]
    public void AcceptInvitation_WrongEmail_Throws()
    {
        var ws = WatchSpace.Create(ValidName, OwnerUserId);
        var invitation = ws.InviteMember("user@example.com", OwnerUserId, DateTime.UtcNow.AddDays(7));

        var act = () => ws.AcceptInvitation(invitation.Token, Guid.NewGuid(), "other@example.com", DateTime.UtcNow);
        act.Should().Throw<WatchSpaceDomainException>();
    }

    // --- DeclineInvitation ---

    [Fact]
    public void DeclineInvitation_Valid_MarksDeclined()
    {
        var ws = WatchSpace.Create(ValidName, OwnerUserId);
        var invitation = ws.InviteMember("user@example.com", OwnerUserId, DateTime.UtcNow.AddDays(7));

        ws.DeclineInvitation(invitation.Token, "user@example.com", DateTime.UtcNow);

        invitation.Status.Should().Be(InvitationStatus.Declined);
        ws.Members.Should().HaveCount(1); // Only owner
    }

    // --- RevokeInvitation ---

    [Fact]
    public void RevokeInvitation_ByOwner_MarksRevoked()
    {
        var ws = WatchSpace.Create(ValidName, OwnerUserId);
        var invitation = ws.InviteMember("user@example.com", OwnerUserId, DateTime.UtcNow.AddDays(7));

        ws.RevokeInvitation(invitation.Id, OwnerUserId);

        invitation.Status.Should().Be(InvitationStatus.Revoked);
    }

    [Fact]
    public void RevokeInvitation_AlreadyAccepted_Throws()
    {
        var ws = WatchSpace.Create(ValidName, OwnerUserId);
        var invitation = ws.InviteMember("user@example.com", OwnerUserId, DateTime.UtcNow.AddDays(7));
        ws.AcceptInvitation(invitation.Token, Guid.NewGuid(), "user@example.com", DateTime.UtcNow);

        var act = () => ws.RevokeInvitation(invitation.Id, OwnerUserId);
        act.Should().Throw<WatchSpaceDomainException>();
    }

    // --- RemoveMember ---

    [Fact]
    public void RemoveMember_ByOwner_RemovesMember()
    {
        var ws = WatchSpace.Create(ValidName, OwnerUserId);
        var invitation = ws.InviteMember("user@example.com", OwnerUserId, DateTime.UtcNow.AddDays(7));
        var memberId = Guid.NewGuid();
        ws.AcceptInvitation(invitation.Token, memberId, "user@example.com", DateTime.UtcNow);

        ws.RemoveMember(memberId, OwnerUserId);

        ws.Members.Should().HaveCount(1);
    }

    [Fact]
    public void RemoveMember_OwnerDirectly_Throws()
    {
        var ws = WatchSpace.Create(ValidName, OwnerUserId);

        var act = () => ws.RemoveMember(OwnerUserId, OwnerUserId);
        act.Should().Throw<WatchSpaceDomainException>();
    }

    [Fact]
    public void RemoveMember_ByNonOwner_Throws()
    {
        var ws = WatchSpace.Create(ValidName, OwnerUserId);
        var act = () => ws.RemoveMember(Guid.NewGuid(), Guid.NewGuid());
        act.Should().Throw<NotAnOwnerException>();
    }

    // --- Leave ---

    [Fact]
    public void Leave_AsMember_RemovesSelf()
    {
        var ws = WatchSpace.Create(ValidName, OwnerUserId);
        var invitation = ws.InviteMember("member@example.com", OwnerUserId, DateTime.UtcNow.AddDays(7));
        var memberId = Guid.NewGuid();
        ws.AcceptInvitation(invitation.Token, memberId, "member@example.com", DateTime.UtcNow);

        ws.Leave(memberId);

        ws.Members.Should().HaveCount(1);
    }

    [Fact]
    public void Leave_AsSoleOwner_Throws()
    {
        var ws = WatchSpace.Create(ValidName, OwnerUserId);
        var act = () => ws.Leave(OwnerUserId);
        act.Should().Throw<WatchSpaceDomainException>();
    }

    // --- TransferOwnership ---

    [Fact]
    public void TransferOwnership_Valid_SwapsRoles()
    {
        var ws = WatchSpace.Create(ValidName, OwnerUserId);
        var invitation = ws.InviteMember("member@example.com", OwnerUserId, DateTime.UtcNow.AddDays(7));
        var memberId = Guid.NewGuid();
        ws.AcceptInvitation(invitation.Token, memberId, "member@example.com", DateTime.UtcNow);

        ws.TransferOwnership(memberId, OwnerUserId);

        ws.Members.First(m => m.UserId == memberId).Role.Should().Be(WatchSpaceRole.Owner);
        ws.Members.First(m => m.UserId == OwnerUserId).Role.Should().Be(WatchSpaceRole.Member);
    }

    [Fact]
    public void TransferOwnership_ToNonMember_Throws()
    {
        var ws = WatchSpace.Create(ValidName, OwnerUserId);
        var act = () => ws.TransferOwnership(Guid.NewGuid(), OwnerUserId);
        act.Should().Throw<WatchSpaceDomainException>();
    }

    [Fact]
    public void TransferOwnership_ByNonOwner_Throws()
    {
        var ws = WatchSpace.Create(ValidName, OwnerUserId);
        var act = () => ws.TransferOwnership(Guid.NewGuid(), Guid.NewGuid());
        act.Should().Throw<NotAnOwnerException>();
    }

    // --- At-least-one-owner invariant ---

    [Fact]
    public void AtLeastOneOwnerInvariant_NewSpace_Satisfied()
    {
        var ws = WatchSpace.Create(ValidName, OwnerUserId);
        ws.Members.Count(m => m.Role == WatchSpaceRole.Owner).Should().Be(1);
    }
}
