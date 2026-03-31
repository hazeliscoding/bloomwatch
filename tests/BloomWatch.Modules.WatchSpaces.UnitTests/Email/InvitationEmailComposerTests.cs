using BloomWatch.Modules.WatchSpaces.Application.Email;
using FluentAssertions;

namespace BloomWatch.Modules.WatchSpaces.UnitTests.Email;

public sealed class InvitationEmailComposerTests
{
    private const string InviterName = "Hazel";
    private const string SpaceName = "Anime Club";
    private const string Token = "abc123token";
    private const string BaseUrl = "https://bloomwatch.app";

    [Fact]
    public void Compose_Subject_ContainsInviterAndSpaceName()
    {
        var (subject, _, _) = InvitationEmailComposer.Compose(InviterName, SpaceName, Token, BaseUrl);

        subject.Should().Be("Hazel invited you to join Anime Club on BloomWatch");
    }

    [Fact]
    public void Compose_AcceptUrl_UsesBaseUrlAndToken()
    {
        var (_, html, plain) = InvitationEmailComposer.Compose(InviterName, SpaceName, Token, BaseUrl);

        var expectedAccept = "https://bloomwatch.app/invitations/abc123token/accept";
        html.Should().Contain(expectedAccept);
        plain.Should().Contain(expectedAccept);
    }

    [Fact]
    public void Compose_DeclineUrl_UsesBaseUrlAndToken()
    {
        var (_, html, plain) = InvitationEmailComposer.Compose(InviterName, SpaceName, Token, BaseUrl);

        var expectedDecline = "https://bloomwatch.app/invitations/abc123token/decline";
        html.Should().Contain(expectedDecline);
        plain.Should().Contain(expectedDecline);
    }

    [Fact]
    public void Compose_HtmlBody_ContainsBrandColor()
    {
        var (_, html, _) = InvitationEmailComposer.Compose(InviterName, SpaceName, Token, BaseUrl);

        html.Should().Contain("#FF6B9D");
    }

    [Fact]
    public void Compose_HtmlBody_ContainsInviterAndSpaceName()
    {
        var (_, html, _) = InvitationEmailComposer.Compose(InviterName, SpaceName, Token, BaseUrl);

        html.Should().Contain(InviterName);
        html.Should().Contain(SpaceName);
    }

    [Fact]
    public void Compose_PlainTextBody_ContainsInviterAndSpaceName()
    {
        var (_, _, plain) = InvitationEmailComposer.Compose(InviterName, SpaceName, Token, BaseUrl);

        plain.Should().Contain(InviterName);
        plain.Should().Contain(SpaceName);
    }

    [Fact]
    public void Compose_PlainTextBody_ContainsNoHtmlTags()
    {
        var (_, _, plain) = InvitationEmailComposer.Compose(InviterName, SpaceName, Token, BaseUrl);

        plain.Should().NotContain("<");
        plain.Should().NotContain(">");
    }

    [Fact]
    public void Compose_BaseUrlWithTrailingSlash_NormalisesLinks()
    {
        var (_, html, plain) = InvitationEmailComposer.Compose(InviterName, SpaceName, Token, "https://bloomwatch.app/");

        html.Should().Contain("https://bloomwatch.app/invitations/abc123token/accept");
        plain.Should().Contain("https://bloomwatch.app/invitations/abc123token/accept");
    }
}
