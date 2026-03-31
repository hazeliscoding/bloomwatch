using BloomWatch.Modules.WatchSpaces.Infrastructure.Email;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace BloomWatch.Modules.WatchSpaces.UnitTests.Email;

public sealed class NoOpInvitationEmailSenderTests
{
    [Fact]
    public async Task SendAsync_CompletesWithoutThrowingOrMakingNetworkCalls()
    {
        var sender = new NoOpInvitationEmailSender(NullLogger<NoOpInvitationEmailSender>.Instance);

        var act = () => sender.SendAsync(
            "test@example.com",
            "token123",
            "Anime Club",
            "Hazel");

        await act.Should().NotThrowAsync();
    }
}
