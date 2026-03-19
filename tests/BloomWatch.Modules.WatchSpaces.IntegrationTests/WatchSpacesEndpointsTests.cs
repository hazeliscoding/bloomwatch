using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace BloomWatch.Modules.WatchSpaces.IntegrationTests;

public sealed class WatchSpacesEndpointsTests : IClassFixture<WatchSpacesWebAppFactory>
{
    private readonly HttpClient _client;
    private readonly WatchSpacesWebAppFactory _factory;

    public WatchSpacesEndpointsTests(WatchSpacesWebAppFactory factory)
    {
        _factory = factory;
        factory.EnsureSchemaCreated();
        _client = factory.CreateClient();
    }

    // --- Helpers ---

    private async Task<string> RegisterAndLoginAsync(string email, string displayName = "Test User")
    {
        await _client.PostAsJsonAsync("/auth/register", new
        {
            email,
            password = "password123",
            displayName
        });

        var loginResponse = await _client.PostAsJsonAsync("/auth/login", new
        {
            email,
            password = "password123"
        });

        var body = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("accessToken").GetString()!;
    }

    private static HttpClient WithToken(HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    // --- Task 7.1: POST /watchspaces ---

    [Fact]
    public async Task CreateWatchSpace_ValidRequest_Returns201AndCreatorIsOwner()
    {
        var token = await RegisterAndLoginAsync($"owner_{Guid.NewGuid()}@example.com");
        WithToken(_client, token);

        var response = await _client.PostAsJsonAsync("/watchspaces", new { name = "My Space" });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("watchSpaceId").GetGuid().Should().NotBeEmpty();
        body.GetProperty("name").GetString().Should().Be("My Space");
        body.GetProperty("role").GetString().Should().Be("Owner");
    }

    [Fact]
    public async Task CreateWatchSpace_NoToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.PostAsJsonAsync("/watchspaces", new { name = "My Space" });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // --- Task 7.2: GET /watchspaces ---

    [Fact]
    public async Task GetMyWatchSpaces_ReturnsOnlyOwnSpaces()
    {
        var email1 = $"spaces_user1_{Guid.NewGuid()}@example.com";
        var email2 = $"spaces_user2_{Guid.NewGuid()}@example.com";

        var token1 = await RegisterAndLoginAsync(email1);
        var token2 = await RegisterAndLoginAsync(email2);

        WithToken(_client, token1);
        await _client.PostAsJsonAsync("/watchspaces", new { name = "User1 Space" });

        WithToken(_client, token2);
        var response = await _client.GetAsync("/watchspaces");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetArrayLength().Should().Be(0); // User2 has no spaces
    }

    // --- Task 7.3: GET /watchspaces/{id} ---

    [Fact]
    public async Task GetWatchSpaceById_AsMember_Returns200WithDisplayName()
    {
        var displayName = "DetailUser";
        var token = await RegisterAndLoginAsync($"getbyid_{Guid.NewGuid()}@example.com", displayName);
        WithToken(_client, token);

        var createResponse = await _client.PostAsJsonAsync("/watchspaces", new { name = "Detail Space" });
        var createBody = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var spaceId = createBody.GetProperty("watchSpaceId").GetGuid();

        var response = await _client.GetAsync($"/watchspaces/{spaceId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("watchSpaceId").GetGuid().Should().Be(spaceId);
        body.GetProperty("members").GetArrayLength().Should().Be(1);

        var member = body.GetProperty("members")[0];
        member.GetProperty("displayName").GetString().Should().Be(displayName);
        member.GetProperty("role").GetString().Should().Be("Owner");
    }

    [Fact]
    public async Task GetWatchSpaceById_AsNonMember_Returns403()
    {
        var ownerToken = await RegisterAndLoginAsync($"owner_getid_{Guid.NewGuid()}@example.com");
        var nonMemberToken = await RegisterAndLoginAsync($"nonmember_getid_{Guid.NewGuid()}@example.com");

        WithToken(_client, ownerToken);
        var createResponse = await _client.PostAsJsonAsync("/watchspaces", new { name = "Private Space" });
        var spaceId = (await createResponse.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("watchSpaceId").GetGuid();

        WithToken(_client, nonMemberToken);
        var response = await _client.GetAsync($"/watchspaces/{spaceId}");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // --- Task 7.4: Full invitation flow ---

    [Fact]
    public async Task InvitationFlow_InviteAccept_MemberAppearsInSpace()
    {
        var ownerEmail = $"owner_inv_{Guid.NewGuid()}@example.com";
        var memberEmail = $"member_inv_{Guid.NewGuid()}@example.com";

        var ownerToken = await RegisterAndLoginAsync(ownerEmail);
        var memberToken = await RegisterAndLoginAsync(memberEmail, "Invited User");

        // Create space
        WithToken(_client, ownerToken);
        var createResponse = await _client.PostAsJsonAsync("/watchspaces", new { name = "Invite Test Space" });
        var spaceId = (await createResponse.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("watchSpaceId").GetGuid();

        // Invite member
        var inviteResponse = await _client.PostAsJsonAsync($"/watchspaces/{spaceId}/invitations", new { email = memberEmail });
        inviteResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var inviteBody = await inviteResponse.Content.ReadFromJsonAsync<JsonElement>();
        var token = inviteBody.GetProperty("token").GetString()!;

        // Accept invitation
        WithToken(_client, memberToken);
        var acceptResponse = await _client.PostAsync($"/watchspaces/invitations/{token}/accept", null);
        acceptResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify member appears in space
        WithToken(_client, ownerToken);
        var spaceResponse = await _client.GetAsync($"/watchspaces/{spaceId}");
        var spaceBody = await spaceResponse.Content.ReadFromJsonAsync<JsonElement>();
        spaceBody.GetProperty("members").GetArrayLength().Should().Be(2);
    }

    // --- Task 7.5: Invitation decline and expiry ---

    [Fact]
    public async Task InvitationFlow_Decline_MemberNotAdded()
    {
        var ownerEmail = $"owner_dec_{Guid.NewGuid()}@example.com";
        var memberEmail = $"member_dec_{Guid.NewGuid()}@example.com";

        var ownerToken = await RegisterAndLoginAsync(ownerEmail);
        var memberToken = await RegisterAndLoginAsync(memberEmail, "Declining User");

        WithToken(_client, ownerToken);
        var createResponse = await _client.PostAsJsonAsync("/watchspaces", new { name = "Decline Test" });
        var spaceId = (await createResponse.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("watchSpaceId").GetGuid();

        var inviteResponse = await _client.PostAsJsonAsync($"/watchspaces/{spaceId}/invitations", new { email = memberEmail });
        var inviteToken = (await inviteResponse.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("token").GetString()!;

        WithToken(_client, memberToken);
        var declineResponse = await _client.PostAsync($"/watchspaces/invitations/{inviteToken}/decline", null);
        declineResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Space should still have only the owner
        WithToken(_client, ownerToken);
        var spaceResponse = await _client.GetAsync($"/watchspaces/{spaceId}");
        var spaceBody = await spaceResponse.Content.ReadFromJsonAsync<JsonElement>();
        spaceBody.GetProperty("members").GetArrayLength().Should().Be(1);
    }

    // --- Task 7.6: RemoveMember ---

    [Fact]
    public async Task RemoveMember_ByOwner_Returns200()
    {
        var ownerEmail = $"owner_rem_{Guid.NewGuid()}@example.com";
        var memberEmail = $"member_rem_{Guid.NewGuid()}@example.com";

        var ownerToken = await RegisterAndLoginAsync(ownerEmail);
        var memberToken = await RegisterAndLoginAsync(memberEmail, "Removable Member");

        WithToken(_client, ownerToken);
        var createResponse = await _client.PostAsJsonAsync("/watchspaces", new { name = "Remove Test" });
        var spaceBody = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var spaceId = spaceBody.GetProperty("watchSpaceId").GetGuid();

        // Invite and accept
        var inviteResponse = await _client.PostAsJsonAsync($"/watchspaces/{spaceId}/invitations", new { email = memberEmail });
        var inviteToken = (await inviteResponse.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("token").GetString()!;

        WithToken(_client, memberToken);
        await _client.PostAsync($"/watchspaces/invitations/{inviteToken}/accept", null);

        // Get member's userId from space detail
        WithToken(_client, ownerToken);
        var spaceDetailResponse = await _client.GetAsync($"/watchspaces/{spaceId}");
        var spaceDetail = await spaceDetailResponse.Content.ReadFromJsonAsync<JsonElement>();
        var members = spaceDetail.GetProperty("members").EnumerateArray().ToList();
        var memberUserId = members.First(m => m.GetProperty("role").GetString() == "Member").GetProperty("userId").GetGuid();

        var removeResponse = await _client.DeleteAsync($"/watchspaces/{spaceId}/members/{memberUserId}");
        removeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // --- Task 7.7: Leave ---

    [Fact]
    public async Task Leave_AsSoleOwner_Returns409()
    {
        var ownerToken = await RegisterAndLoginAsync($"sole_owner_{Guid.NewGuid()}@example.com");
        WithToken(_client, ownerToken);

        var createResponse = await _client.PostAsJsonAsync("/watchspaces", new { name = "Leave Test" });
        var spaceId = (await createResponse.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("watchSpaceId").GetGuid();

        var leaveResponse = await _client.DeleteAsync($"/watchspaces/{spaceId}/members/me");
        leaveResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    // --- Task 7.8: TransferOwnership ---

    [Fact]
    public async Task TransferOwnership_RolesSwapCorrectly()
    {
        var ownerEmail = $"owner_to_{Guid.NewGuid()}@example.com";
        var memberEmail = $"member_to_{Guid.NewGuid()}@example.com";

        var ownerToken = await RegisterAndLoginAsync(ownerEmail);
        var memberToken = await RegisterAndLoginAsync(memberEmail, "New Owner");

        WithToken(_client, ownerToken);
        var createResponse = await _client.PostAsJsonAsync("/watchspaces", new { name = "Transfer Space" });
        var spaceId = (await createResponse.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("watchSpaceId").GetGuid();

        var inviteResponse = await _client.PostAsJsonAsync($"/watchspaces/{spaceId}/invitations", new { email = memberEmail });
        var inviteToken = (await inviteResponse.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("token").GetString()!;

        WithToken(_client, memberToken);
        await _client.PostAsync($"/watchspaces/invitations/{inviteToken}/accept", null);

        // Get the new member's userId
        WithToken(_client, ownerToken);
        var spaceDetailResponse = await _client.GetAsync($"/watchspaces/{spaceId}");
        var spaceDetail = await spaceDetailResponse.Content.ReadFromJsonAsync<JsonElement>();
        var members = spaceDetail.GetProperty("members").EnumerateArray().ToList();
        var newOwnerId = members.First(m => m.GetProperty("role").GetString() == "Member").GetProperty("userId").GetGuid();

        var transferResponse = await _client.PostAsJsonAsync(
            $"/watchspaces/{spaceId}/transfer-ownership",
            new { newOwnerId });
        transferResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify roles swapped
        var afterResponse = await _client.GetAsync($"/watchspaces/{spaceId}");
        var afterBody = await afterResponse.Content.ReadFromJsonAsync<JsonElement>();
        var afterMembers = afterBody.GetProperty("members").EnumerateArray().ToList();
        afterMembers.First(m => m.GetProperty("userId").GetGuid() == newOwnerId)
            .GetProperty("role").GetString().Should().Be("Owner");
    }
}
