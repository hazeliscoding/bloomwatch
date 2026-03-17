using System.Security.Claims;
using BloomWatch.Modules.AnimeTracking.Application.UseCases.AddAnimeToWatchSpace;
using BloomWatch.Modules.AnimeTracking.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace BloomWatch.Api.Modules.AnimeTracking;

public static class AnimeTrackingEndpoints
{
    public static IEndpointRouteBuilder MapAnimeTrackingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/watchspaces/{watchSpaceId:guid}/anime")
            .WithTags("AnimeTracking")
            .RequireAuthorization();

        group.MapPost("/", AddAnimeAsync)
            .WithName("AddAnimeToWatchSpace")
            .WithSummary("Add an anime to a watch space")
            .WithDescription(
                "Adds an anime by its AniList media ID to the specified watch space. " +
                "The caller must be a member of the watch space. " +
                "Metadata is snapshotted from the local AniList cache.")
            .Produces<AddAnimeToWatchSpaceResult>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);

        return app;
    }

    private static async Task<IResult> AddAnimeAsync(
        Guid watchSpaceId,
        [FromBody] AddAnimeRequest request,
        ClaimsPrincipal user,
        AddAnimeToWatchSpaceCommandHandler handler,
        CancellationToken ct)
    {
        var userId = GetUserId(user);
        try
        {
            var result = await handler.HandleAsync(
                new AddAnimeToWatchSpaceCommand(
                    watchSpaceId,
                    request.AniListMediaId,
                    request.Mood,
                    request.Vibe,
                    request.Pitch,
                    userId),
                ct);

            return Results.Created($"/watchspaces/{watchSpaceId}/anime/{result.WatchSpaceAnimeId}", result);
        }
        catch (NotAWatchSpaceMemberException)
        {
            return Results.Forbid();
        }
        catch (AnimeAlreadyInWatchSpaceException ex)
        {
            return Results.Conflict(new { error = ex.Message });
        }
        catch (MediaNotFoundException)
        {
            return Results.NotFound(new { error = "AniList media not found in cache." });
        }
    }

    private static Guid GetUserId(ClaimsPrincipal user)
    {
        var sub = user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue("sub")
            ?? throw new InvalidOperationException("User ID claim not found.");
        return Guid.Parse(sub);
    }
}

/// <summary>
/// Request body for adding an anime to a watch space.
/// </summary>
public sealed record AddAnimeRequest(
    int AniListMediaId,
    string? Mood = null,
    string? Vibe = null,
    string? Pitch = null);
