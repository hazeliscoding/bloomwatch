using System.Security.Claims;
using BloomWatch.Modules.WatchSpaces.Application.UseCases.AcceptInvitation;
using BloomWatch.Modules.WatchSpaces.Application.UseCases.CreateWatchSpace;
using BloomWatch.Modules.WatchSpaces.Application.UseCases.DeclineInvitation;
using BloomWatch.Modules.WatchSpaces.Application.UseCases.GetInvitationByToken;
using BloomWatch.Modules.WatchSpaces.Application.UseCases.GetMyWatchSpaces;
using BloomWatch.Modules.WatchSpaces.Application.UseCases.GetWatchSpaceById;
using BloomWatch.Modules.WatchSpaces.Application.UseCases.InviteMember;
using BloomWatch.Modules.WatchSpaces.Application.UseCases.LeaveWatchSpace;
using BloomWatch.Modules.WatchSpaces.Application.UseCases.ListInvitations;
using BloomWatch.Modules.WatchSpaces.Application.UseCases.RemoveMember;
using BloomWatch.Modules.WatchSpaces.Application.UseCases.RenameWatchSpace;
using BloomWatch.Modules.WatchSpaces.Application.UseCases.RevokeInvitation;
using BloomWatch.Modules.WatchSpaces.Application.UseCases.TransferOwnership;
using BloomWatch.Modules.WatchSpaces.Domain.Aggregates;
using BloomWatch.Modules.WatchSpaces.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BloomWatch.Api.Modules.WatchSpaces;

/// <summary>
/// Defines the minimal API endpoints for the WatchSpaces module, covering space management,
/// member invitations, and membership operations.
/// </summary>
public static class WatchSpacesEndpoints
{
    /// <summary>
    /// Maps the WatchSpaces HTTP endpoints onto the application's routing pipeline.
    /// </summary>
    /// <remarks>
    /// <para>All endpoints under <c>/watchspaces</c> require authorization. Registers the following route groups:</para>
    /// <list type="bullet">
    ///   <item><description>Space management: create, list, get by ID, rename, transfer ownership.</description></item>
    ///   <item><description>Invitations (owner): invite members, list invitations, revoke invitations.</description></item>
    ///   <item><description>Invitations (invitee): accept or decline an invitation by token.</description></item>
    ///   <item><description>Membership: remove a member, leave a space.</description></item>
    /// </list>
    /// </remarks>
    /// <param name="app">The endpoint route builder to add the WatchSpaces routes to.</param>
    /// <returns>The same <see cref="IEndpointRouteBuilder"/> instance for chaining.</returns>
    public static IEndpointRouteBuilder MapWatchSpacesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/watchspaces").WithTags("WatchSpaces").RequireAuthorization();
        var invitationsGroup = app.MapGroup("/watchspaces/invitations").WithTags("WatchSpaces");

        // Space management
        group.MapPost("/", CreateAsync)
            .WithName("CreateWatchSpace")
            .WithSummary("Create a new WatchSpace")
            .WithDescription(
                "Creates a new WatchSpace owned by the authenticated user. " +
                "The caller is automatically added as the Owner member.")
            .Produces<CreateWatchSpaceResult>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapGet("/", GetMySpacesAsync)
            .WithName("GetMyWatchSpaces")
            .WithSummary("List WatchSpaces for the current user")
            .WithDescription(
                "Returns all WatchSpaces the authenticated user belongs to, " +
                "including their role in each space.")
            .Produces<IReadOnlyList<WatchSpaceSummary>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", GetByIdAsync)
            .WithName("GetWatchSpaceById")
            .WithSummary("Get a WatchSpace by ID")
            .WithDescription(
                "Returns the full details of a single WatchSpace, including its member list. " +
                "The authenticated user must be a member of the space.")
            .Produces<WatchSpaceDetail>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapPatch("/{id:guid}", RenameAsync)
            .WithName("RenameWatchSpace")
            .WithSummary("Rename a WatchSpace")
            .WithDescription(
                "Updates the name of the specified WatchSpace. " +
                "Only the Owner may perform this action.")
            .Produces<RenameWatchSpaceResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/{id:guid}/transfer-ownership", TransferOwnershipAsync)
            .WithName("TransferOwnership")
            .WithSummary("Transfer WatchSpace ownership to another member")
            .WithDescription(
                "Assigns ownership of the WatchSpace to a different member. " +
                "The requesting user must currently be the Owner. " +
                "The target user must already be a member of the space.")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status400BadRequest);

        // Invitations (owner actions)
        group.MapPost("/{id:guid}/invitations", InviteAsync)
            .WithName("InviteMember")
            .WithSummary("Invite a user to a WatchSpace")
            .WithDescription(
                "Sends an invitation to the specified email address. " +
                "Only the Owner may send invitations. " +
                "The invitation includes a unique token that the recipient uses to accept or decline. " +
                "If email delivery fails after retries, the invitation is still created and " +
                "`emailDeliveryFailed: true` is returned so the owner can notify the invitee manually.")
            .Produces<InviteMemberResult>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status409Conflict)
            .Produces(StatusCodes.Status422UnprocessableEntity);

        group.MapGet("/{id:guid}/invitations", ListInvitationsAsync)
            .WithName("ListInvitations")
            .WithSummary("List pending invitations for a WatchSpace")
            .WithDescription(
                "Returns all invitations (pending, accepted, declined, expired) for the given WatchSpace. " +
                "Only the Owner may view invitations.")
            .Produces<IReadOnlyList<InvitationDetail>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapDelete("/{id:guid}/invitations/{invitationId:guid}", RevokeInvitationAsync)
            .WithName("RevokeInvitation")
            .WithSummary("Revoke a pending invitation")
            .WithDescription(
                "Cancels a pending invitation so it can no longer be accepted. " +
                "Only the Owner may revoke invitations.")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status409Conflict);

        // Invitations (invitee actions -- no space ID needed, token identifies the invitation)
        invitationsGroup.MapGet("/{token}", GetInvitationByTokenAsync)
            .WithName("GetInvitationByToken")
            .WithSummary("Preview a WatchSpace invitation")
            .WithDescription(
                "Returns the invitation details (watch space name, status, expiry) " +
                "for the given token. Used by the invitee to preview the invitation before acting.")
            .Produces<InvitationPreviewResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        invitationsGroup.MapPost("/{token}/accept", AcceptInvitationAsync)
            .WithName("AcceptInvitation")
            .WithSummary("Accept a WatchSpace invitation")
            .WithDescription(
                "The authenticated user accepts the invitation identified by the provided token. " +
                "The user's email must match the email the invitation was addressed to. " +
                "Returns 410 Gone if the invitation has expired.")
            .Produces<AcceptInvitationResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status410Gone)
            .Produces(StatusCodes.Status409Conflict)
            .Produces(StatusCodes.Status403Forbidden)
            .RequireAuthorization();

        invitationsGroup.MapPost("/{token}/decline", DeclineInvitationAsync)
            .WithName("DeclineInvitation")
            .WithSummary("Decline a WatchSpace invitation")
            .WithDescription(
                "The authenticated user declines the invitation identified by the provided token. " +
                "The user's email must match the email the invitation was addressed to.")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization();

        // Membership
        group.MapDelete("/{id:guid}/members/{userId:guid}", RemoveMemberAsync)
            .WithName("RemoveMember")
            .WithSummary("Remove a member from a WatchSpace")
            .WithDescription(
                "Removes the specified user from the WatchSpace. " +
                "Only the Owner may remove members. The Owner cannot remove themselves.")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status409Conflict);

        group.MapDelete("/{id:guid}/members/me", LeaveAsync)
            .WithName("LeaveWatchSpace")
            .WithSummary("Leave a WatchSpace")
            .WithDescription(
                "Removes the authenticated user from the WatchSpace. " +
                "The Owner cannot leave; they must transfer ownership first.")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);

        return app;
    }

    // --- Space management ---

    /// <summary>
    /// Handles the request to create a new WatchSpace owned by the authenticated user.
    /// </summary>
    private static async Task<IResult> CreateAsync(
        [FromBody] CreateWatchSpaceRequest request,
        ClaimsPrincipal user,
        ISender sender,
        CancellationToken ct)
    {
        var userId = GetUserId(user);
        try
        {
            var result = await sender.Send(new CreateWatchSpaceCommand(request.Name, userId), ct);
            return Results.Created($"/watchspaces/{result.WatchSpaceId}", result);
        }
        catch (WatchSpaceDomainException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Handles the request to list all WatchSpaces the authenticated user belongs to.
    /// </summary>
    private static async Task<IResult> GetMySpacesAsync(
        ClaimsPrincipal user,
        ISender sender,
        CancellationToken ct)
    {
        var userId = GetUserId(user);
        var result = await sender.Send(new GetMyWatchSpacesQuery(userId), ct);
        return Results.Ok(result);
    }

    /// <summary>
    /// Handles the request to retrieve a single WatchSpace by its unique identifier.
    /// </summary>
    private static async Task<IResult> GetByIdAsync(
        Guid id,
        ClaimsPrincipal user,
        ISender sender,
        CancellationToken ct)
    {
        var userId = GetUserId(user);
        try
        {
            var result = await sender.Send(new GetWatchSpaceByIdQuery(id, userId), ct);
            return Results.Ok(result);
        }
        catch (WatchSpaceNotFoundException)
        {
            return Results.NotFound();
        }
        catch (NotAMemberException)
        {
            return Results.Forbid();
        }
    }

    /// <summary>
    /// Handles the request to rename a WatchSpace. Only the owner may perform this action.
    /// </summary>
    private static async Task<IResult> RenameAsync(
        Guid id,
        [FromBody] RenameWatchSpaceRequest request,
        ClaimsPrincipal user,
        ISender sender,
        CancellationToken ct)
    {
        var userId = GetUserId(user);
        try
        {
            var result = await sender.Send(new RenameWatchSpaceCommand(id, request.Name, userId), ct);
            return Results.Ok(result);
        }
        catch (WatchSpaceNotFoundException)
        {
            return Results.NotFound();
        }
        catch (NotAnOwnerException)
        {
            return Results.Forbid();
        }
        catch (WatchSpaceDomainException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Handles the request to transfer WatchSpace ownership to another member.
    /// </summary>
    private static async Task<IResult> TransferOwnershipAsync(
        Guid id,
        [FromBody] TransferOwnershipRequest request,
        ClaimsPrincipal user,
        ISender sender,
        CancellationToken ct)
    {
        var userId = GetUserId(user);
        try
        {
            await sender.Send(new TransferOwnershipCommand(id, request.NewOwnerId, userId), ct);
            return Results.Ok();
        }
        catch (WatchSpaceNotFoundException)
        {
            return Results.NotFound();
        }
        catch (NotAnOwnerException)
        {
            return Results.Forbid();
        }
        catch (WatchSpaceDomainException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    // --- Invitations (owner actions) ---

    /// <summary>
    /// Handles the request to invite a user to a WatchSpace by email address.
    /// </summary>
    private static async Task<IResult> InviteAsync(
        Guid id,
        [FromBody] InviteMemberRequest request,
        ClaimsPrincipal user,
        ISender sender,
        CancellationToken ct)
    {
        var userId = GetUserId(user);
        try
        {
            var result = await sender.Send(new InviteMemberCommand(id, request.Email, userId), ct);
            return Results.Created($"/watchspaces/{id}/invitations/{result.InvitationId}", result);
        }
        catch (WatchSpaceNotFoundException)
        {
            return Results.NotFound();
        }
        catch (NotAnOwnerException)
        {
            return Results.Forbid();
        }
        catch (AlreadyAMemberException ex)
        {
            return Results.Conflict(new { error = ex.Message });
        }
        catch (InvitedUserNotFoundException ex)
        {
            return Results.UnprocessableEntity(new { error = ex.Message });
        }
        catch (WatchSpaceDomainException ex)
        {
            return Results.Conflict(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Handles the request to list all invitations for a WatchSpace. Only the owner may view invitations.
    /// </summary>
    private static async Task<IResult> ListInvitationsAsync(
        Guid id,
        ClaimsPrincipal user,
        ISender sender,
        CancellationToken ct)
    {
        var userId = GetUserId(user);
        try
        {
            var result = await sender.Send(new ListInvitationsQuery(id, userId), ct);
            return Results.Ok(result);
        }
        catch (WatchSpaceNotFoundException)
        {
            return Results.NotFound();
        }
        catch (NotAnOwnerException)
        {
            return Results.Forbid();
        }
    }

    /// <summary>
    /// Handles the request to revoke a pending invitation. Only the owner may revoke invitations.
    /// </summary>
    private static async Task<IResult> RevokeInvitationAsync(
        Guid id,
        Guid invitationId,
        ClaimsPrincipal user,
        ISender sender,
        CancellationToken ct)
    {
        var userId = GetUserId(user);
        try
        {
            await sender.Send(new RevokeInvitationCommand(id, invitationId, userId), ct);
            return Results.Ok();
        }
        catch (WatchSpaceNotFoundException)
        {
            return Results.NotFound();
        }
        catch (NotAnOwnerException)
        {
            return Results.Forbid();
        }
        catch (WatchSpaceDomainException ex)
        {
            return Results.Conflict(new { error = ex.Message });
        }
    }

    // --- Invitations (invitee actions) ---

    /// <summary>
    /// Handles the request to preview a WatchSpace invitation by its token.
    /// </summary>
    private static async Task<IResult> GetInvitationByTokenAsync(
        string token,
        ClaimsPrincipal user,
        ISender sender,
        CancellationToken ct)
    {
        var email = GetEmail(user);
        try
        {
            var result = await sender.Send(new GetInvitationByTokenQuery(token, email), ct);
            return Results.Ok(result);
        }
        catch (InvitationNotFoundException)
        {
            return Results.NotFound();
        }
    }

    /// <summary>
    /// Handles the request to accept a WatchSpace invitation using the invitation token.
    /// </summary>
    private static async Task<IResult> AcceptInvitationAsync(
        string token,
        ClaimsPrincipal user,
        ISender sender,
        CancellationToken ct)
    {
        var userId = GetUserId(user);
        var email = GetEmail(user);
        try
        {
            var result = await sender.Send(new AcceptInvitationCommand(token, userId, email), ct);
            return Results.Ok(result);
        }
        catch (InvitationNotFoundException)
        {
            return Results.NotFound();
        }
        catch (InvitationExpiredException)
        {
            return Results.StatusCode(410);
        }
        catch (InvitationAlreadyProcessedException ex)
        {
            return Results.Conflict(new { error = ex.Message });
        }
        catch (WatchSpaceDomainException)
        {
            return Results.Forbid();
        }
    }

    /// <summary>
    /// Handles the request to decline a WatchSpace invitation using the invitation token.
    /// </summary>
    private static async Task<IResult> DeclineInvitationAsync(
        string token,
        ClaimsPrincipal user,
        ISender sender,
        CancellationToken ct)
    {
        var email = GetEmail(user);
        try
        {
            await sender.Send(new DeclineInvitationCommand(token, email), ct);
            return Results.Ok();
        }
        catch (InvitationNotFoundException)
        {
            return Results.NotFound();
        }
        catch (WatchSpaceDomainException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    // --- Membership ---

    /// <summary>
    /// Handles the request to remove a member from a WatchSpace. Only the owner may remove members.
    /// </summary>
    private static async Task<IResult> RemoveMemberAsync(
        Guid id,
        Guid userId,
        ClaimsPrincipal user,
        ISender sender,
        CancellationToken ct)
    {
        var requestingUserId = GetUserId(user);
        try
        {
            await sender.Send(new RemoveMemberCommand(id, userId, requestingUserId), ct);
            return Results.Ok();
        }
        catch (WatchSpaceNotFoundException)
        {
            return Results.NotFound();
        }
        catch (NotAnOwnerException)
        {
            return Results.Forbid();
        }
        catch (MemberNotFoundException)
        {
            return Results.NotFound();
        }
        catch (WatchSpaceDomainException ex)
        {
            return Results.Conflict(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Handles the request for the authenticated user to leave a WatchSpace voluntarily.
    /// </summary>
    private static async Task<IResult> LeaveAsync(
        Guid id,
        ClaimsPrincipal user,
        ISender sender,
        CancellationToken ct)
    {
        var userId = GetUserId(user);
        try
        {
            await sender.Send(new LeaveWatchSpaceCommand(id, userId), ct);
            return Results.Ok();
        }
        catch (WatchSpaceNotFoundException)
        {
            return Results.NotFound();
        }
        catch (WatchSpaceDomainException ex)
        {
            return Results.Conflict(new { error = ex.Message });
        }
    }

    // --- Helpers ---

    /// <summary>
    /// Extracts the user's unique identifier from the JWT claims principal.
    /// </summary>
    /// <param name="user">The claims principal representing the authenticated user.</param>
    /// <returns>The user's <see cref="Guid"/> identifier.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the user ID claim is not present in the token.</exception>
    private static Guid GetUserId(ClaimsPrincipal user)
    {
        var sub = user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue("sub")
            ?? throw new InvalidOperationException("User ID claim not found.");
        return Guid.Parse(sub);
    }

    /// <summary>
    /// Extracts the user's email address from the JWT claims principal.
    /// </summary>
    /// <param name="user">The claims principal representing the authenticated user.</param>
    /// <returns>The user's email address as a string.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the email claim is not present in the token.</exception>
    private static string GetEmail(ClaimsPrincipal user)
        => user.FindFirstValue(ClaimTypes.Email)
            ?? user.FindFirstValue("email")
            ?? throw new InvalidOperationException("Email claim not found.");
}

// Request DTOs

/// <summary>
/// Represents the request body for creating a new WatchSpace.
/// </summary>
/// <param name="Name">The display name for the new WatchSpace.</param>
public sealed record CreateWatchSpaceRequest(string Name);

/// <summary>
/// Represents the request body for renaming an existing WatchSpace.
/// </summary>
/// <param name="Name">The new display name for the WatchSpace.</param>
public sealed record RenameWatchSpaceRequest(string Name);

/// <summary>
/// Represents the request body for inviting a user to a WatchSpace.
/// </summary>
/// <param name="Email">The email address of the user to invite.</param>
public sealed record InviteMemberRequest(string Email);

/// <summary>
/// Represents the request body for transferring ownership of a WatchSpace.
/// </summary>
/// <param name="NewOwnerId">The unique identifier of the member who will become the new owner.</param>
public sealed record TransferOwnershipRequest(Guid NewOwnerId);
