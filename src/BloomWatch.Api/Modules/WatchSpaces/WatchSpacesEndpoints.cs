using System.Security.Claims;
using BloomWatch.Modules.WatchSpaces.Application.UseCases.AcceptInvitation;
using BloomWatch.Modules.WatchSpaces.Application.UseCases.CreateWatchSpace;
using BloomWatch.Modules.WatchSpaces.Application.UseCases.DeclineInvitation;
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
using Microsoft.AspNetCore.Mvc;

namespace BloomWatch.Api.Modules.WatchSpaces;

public static class WatchSpacesEndpoints
{
    public static IEndpointRouteBuilder MapWatchSpacesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/watchspaces").WithTags("WatchSpaces").RequireAuthorization();
        var invitationsGroup = app.MapGroup("/watchspaces/invitations").WithTags("WatchSpaces");

        // Space management
        group.MapPost("/", CreateAsync);
        group.MapGet("/", GetMySpacesAsync);
        group.MapGet("/{id:guid}", GetByIdAsync);
        group.MapPatch("/{id:guid}", RenameAsync);
        group.MapPost("/{id:guid}/transfer-ownership", TransferOwnershipAsync);

        // Invitations (owner actions)
        group.MapPost("/{id:guid}/invitations", InviteAsync);
        group.MapGet("/{id:guid}/invitations", ListInvitationsAsync);
        group.MapDelete("/{id:guid}/invitations/{invitationId:guid}", RevokeInvitationAsync);

        // Invitations (invitee actions — no space ID needed, token identifies the invitation)
        invitationsGroup.MapPost("/{token}/accept", AcceptInvitationAsync).RequireAuthorization();
        invitationsGroup.MapPost("/{token}/decline", DeclineInvitationAsync).RequireAuthorization();

        // Membership
        group.MapDelete("/{id:guid}/members/{userId:guid}", RemoveMemberAsync);
        group.MapDelete("/{id:guid}/members/me", LeaveAsync);

        return app;
    }

    // --- Space management ---

    private static async Task<IResult> CreateAsync(
        [FromBody] CreateWatchSpaceRequest request,
        ClaimsPrincipal user,
        CreateWatchSpaceCommandHandler handler,
        CancellationToken ct)
    {
        var userId = GetUserId(user);
        try
        {
            var result = await handler.HandleAsync(new CreateWatchSpaceCommand(request.Name, userId), ct);
            return Results.Created($"/watchspaces/{result.WatchSpaceId}", result);
        }
        catch (WatchSpaceDomainException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> GetMySpacesAsync(
        ClaimsPrincipal user,
        GetMyWatchSpacesQueryHandler handler,
        CancellationToken ct)
    {
        var userId = GetUserId(user);
        var result = await handler.HandleAsync(new GetMyWatchSpacesQuery(userId), ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        ClaimsPrincipal user,
        GetWatchSpaceByIdQueryHandler handler,
        CancellationToken ct)
    {
        var userId = GetUserId(user);
        try
        {
            var result = await handler.HandleAsync(new GetWatchSpaceByIdQuery(id, userId), ct);
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

    private static async Task<IResult> RenameAsync(
        Guid id,
        [FromBody] RenameWatchSpaceRequest request,
        ClaimsPrincipal user,
        RenameWatchSpaceCommandHandler handler,
        CancellationToken ct)
    {
        var userId = GetUserId(user);
        try
        {
            var result = await handler.HandleAsync(new RenameWatchSpaceCommand(id, request.Name, userId), ct);
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

    private static async Task<IResult> TransferOwnershipAsync(
        Guid id,
        [FromBody] TransferOwnershipRequest request,
        ClaimsPrincipal user,
        TransferOwnershipCommandHandler handler,
        CancellationToken ct)
    {
        var userId = GetUserId(user);
        try
        {
            await handler.HandleAsync(new TransferOwnershipCommand(id, request.NewOwnerId, userId), ct);
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

    private static async Task<IResult> InviteAsync(
        Guid id,
        [FromBody] InviteMemberRequest request,
        ClaimsPrincipal user,
        InviteMemberCommandHandler handler,
        CancellationToken ct)
    {
        var userId = GetUserId(user);
        try
        {
            var result = await handler.HandleAsync(new InviteMemberCommand(id, request.Email, userId), ct);
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

    private static async Task<IResult> ListInvitationsAsync(
        Guid id,
        ClaimsPrincipal user,
        ListInvitationsQueryHandler handler,
        CancellationToken ct)
    {
        var userId = GetUserId(user);
        try
        {
            var result = await handler.HandleAsync(new ListInvitationsQuery(id, userId), ct);
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

    private static async Task<IResult> RevokeInvitationAsync(
        Guid id,
        Guid invitationId,
        ClaimsPrincipal user,
        RevokeInvitationCommandHandler handler,
        CancellationToken ct)
    {
        var userId = GetUserId(user);
        try
        {
            await handler.HandleAsync(new RevokeInvitationCommand(id, invitationId, userId), ct);
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

    private static async Task<IResult> AcceptInvitationAsync(
        string token,
        ClaimsPrincipal user,
        AcceptInvitationCommandHandler handler,
        CancellationToken ct)
    {
        var userId = GetUserId(user);
        var email = GetEmail(user);
        try
        {
            await handler.HandleAsync(new AcceptInvitationCommand(token, userId, email), ct);
            return Results.Ok();
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

    private static async Task<IResult> DeclineInvitationAsync(
        string token,
        ClaimsPrincipal user,
        DeclineInvitationCommandHandler handler,
        CancellationToken ct)
    {
        var email = GetEmail(user);
        try
        {
            await handler.HandleAsync(new DeclineInvitationCommand(token, email), ct);
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

    private static async Task<IResult> RemoveMemberAsync(
        Guid id,
        Guid userId,
        ClaimsPrincipal user,
        RemoveMemberCommandHandler handler,
        CancellationToken ct)
    {
        var requestingUserId = GetUserId(user);
        try
        {
            await handler.HandleAsync(new RemoveMemberCommand(id, userId, requestingUserId), ct);
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

    private static async Task<IResult> LeaveAsync(
        Guid id,
        ClaimsPrincipal user,
        LeaveWatchSpaceCommandHandler handler,
        CancellationToken ct)
    {
        var userId = GetUserId(user);
        try
        {
            await handler.HandleAsync(new LeaveWatchSpaceCommand(id, userId), ct);
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

    private static Guid GetUserId(ClaimsPrincipal user)
    {
        var sub = user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue("sub")
            ?? throw new InvalidOperationException("User ID claim not found.");
        return Guid.Parse(sub);
    }

    private static string GetEmail(ClaimsPrincipal user)
        => user.FindFirstValue(ClaimTypes.Email)
            ?? user.FindFirstValue("email")
            ?? throw new InvalidOperationException("Email claim not found.");
}

// Request DTOs
public sealed record CreateWatchSpaceRequest(string Name);
public sealed record RenameWatchSpaceRequest(string Name);
public sealed record InviteMemberRequest(string Email);
public sealed record TransferOwnershipRequest(Guid NewOwnerId);
