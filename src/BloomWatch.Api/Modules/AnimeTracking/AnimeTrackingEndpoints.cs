using System.Security.Claims;
using BloomWatch.Modules.AnimeTracking.Application.UseCases.AddAnimeToWatchSpace;
using BloomWatch.Modules.AnimeTracking.Application.UseCases.GetWatchSpaceAnimeDetail;
using BloomWatch.Modules.AnimeTracking.Application.UseCases.ListWatchSpaceAnime;
using BloomWatch.Modules.AnimeTracking.Application.UseCases.UpdateParticipantProgress;
using BloomWatch.Modules.AnimeTracking.Application.UseCases.UpdateParticipantRating;
using BloomWatch.Modules.AnimeTracking.Application.UseCases.UpdateSharedAnimeStatus;
using BloomWatch.Modules.AnimeTracking.Domain.Enums;
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

        group.MapGet("/", ListAnimeAsync)
            .WithName("ListWatchSpaceAnime")
            .WithSummary("List all anime in a watch space")
            .WithDescription(
                "Returns all anime tracked in the specified watch space with participant summaries. " +
                "Supports optional status filtering. The caller must be a member of the watch space.")
            .Produces<ListWatchSpaceAnimeResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapGet("/{watchSpaceAnimeId:guid}", GetAnimeDetailAsync)
            .WithName("GetWatchSpaceAnimeDetail")
            .WithSummary("Get full detail for a single anime in a watch space")
            .WithDescription(
                "Returns the full aggregate for a single tracked anime including all participant entries " +
                "(with ratings) and watch session history. The caller must be a member of the watch space.")
            .Produces<GetWatchSpaceAnimeDetailResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPatch("/{watchSpaceAnimeId:guid}", UpdateSharedAnimeStatusAsync)
            .WithName("UpdateSharedAnimeStatus")
            .WithSummary("Update the shared status and metadata for an anime in a watch space")
            .WithDescription(
                "Partially updates the shared tracking state (status, episodes watched, mood, vibe, pitch) " +
                "for an anime in the specified watch space. Only provided fields are updated. " +
                "The caller must be a member of the watch space.")
            .Produces<GetWatchSpaceAnimeDetailResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPatch("/{watchSpaceAnimeId:guid}/participant-progress", UpdateParticipantProgressAsync)
            .WithName("UpdateParticipantProgress")
            .WithSummary("Update the caller's individual progress for an anime in a watch space")
            .WithDescription(
                "Updates the requesting user's individual status and episodes watched for an anime " +
                "in the specified watch space. Creates the participant entry if it does not already exist.")
            .Produces<ParticipantDetailResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPatch("/{watchSpaceAnimeId:guid}/participant-rating", UpdateParticipantRatingAsync)
            .WithName("UpdateParticipantRating")
            .WithSummary("Submit or update the caller's personal rating for an anime in a watch space")
            .WithDescription(
                "Updates the requesting user's rating score and optional notes for an anime " +
                "in the specified watch space. Creates the participant entry if it does not already exist.")
            .Produces<ParticipantDetailResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

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

    private static async Task<IResult> ListAnimeAsync(
        Guid watchSpaceId,
        [FromQuery] AnimeStatus? status,
        ClaimsPrincipal user,
        ListWatchSpaceAnimeQueryHandler handler,
        CancellationToken ct)
    {
        var userId = GetUserId(user);
        try
        {
            var result = await handler.HandleAsync(
                new ListWatchSpaceAnimeQuery(watchSpaceId, status, userId), ct);

            return Results.Ok(result);
        }
        catch (NotAWatchSpaceMemberException)
        {
            return Results.Forbid();
        }
    }

    private static async Task<IResult> GetAnimeDetailAsync(
        Guid watchSpaceId,
        Guid watchSpaceAnimeId,
        ClaimsPrincipal user,
        GetWatchSpaceAnimeDetailQueryHandler handler,
        CancellationToken ct)
    {
        var userId = GetUserId(user);
        try
        {
            var result = await handler.HandleAsync(
                new GetWatchSpaceAnimeDetailQuery(watchSpaceId, watchSpaceAnimeId, userId), ct);

            return result is null
                ? Results.NotFound(new { error = "Anime not found in this watch space." })
                : Results.Ok(result);
        }
        catch (NotAWatchSpaceMemberException)
        {
            return Results.Forbid();
        }
    }

    private static async Task<IResult> UpdateSharedAnimeStatusAsync(
        Guid watchSpaceId,
        Guid watchSpaceAnimeId,
        [FromBody] UpdateSharedAnimeStatusRequest request,
        ClaimsPrincipal user,
        UpdateSharedAnimeStatusCommandHandler handler,
        CancellationToken ct)
    {
        var userId = GetUserId(user);

        AnimeStatus? parsedStatus = null;
        if (request.SharedStatus is not null)
        {
            if (!Enum.TryParse<AnimeStatus>(request.SharedStatus, ignoreCase: true, out var status))
                return Results.BadRequest(new { error = $"Invalid status value '{request.SharedStatus}'." });
            parsedStatus = status;
        }

        try
        {
            var result = await handler.HandleAsync(
                new UpdateSharedAnimeStatusCommand(
                    watchSpaceId,
                    watchSpaceAnimeId,
                    userId,
                    parsedStatus,
                    request.SharedEpisodesWatched,
                    request.Mood,
                    request.Vibe,
                    request.Pitch),
                ct);

            return result is null
                ? Results.NotFound(new { error = "Anime not found in this watch space." })
                : Results.Ok(result);
        }
        catch (NotAWatchSpaceMemberException)
        {
            return Results.Forbid();
        }
        catch (AnimeTrackingDomainException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> UpdateParticipantProgressAsync(
        Guid watchSpaceId,
        Guid watchSpaceAnimeId,
        [FromBody] UpdateParticipantProgressRequest request,
        ClaimsPrincipal user,
        UpdateParticipantProgressCommandHandler handler,
        CancellationToken ct)
    {
        var userId = GetUserId(user);

        if (!Enum.TryParse<AnimeStatus>(request.IndividualStatus, ignoreCase: true, out var parsedStatus))
            return Results.BadRequest(new { error = $"Invalid status value '{request.IndividualStatus}'." });

        try
        {
            var result = await handler.HandleAsync(
                new UpdateParticipantProgressCommand(
                    watchSpaceId,
                    watchSpaceAnimeId,
                    userId,
                    parsedStatus,
                    request.EpisodesWatched),
                ct);

            return result is null
                ? Results.NotFound(new { error = "Anime not found in this watch space." })
                : Results.Ok(result);
        }
        catch (NotAWatchSpaceMemberException)
        {
            return Results.Forbid();
        }
        catch (AnimeTrackingDomainException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> UpdateParticipantRatingAsync(
        Guid watchSpaceId,
        Guid watchSpaceAnimeId,
        [FromBody] UpdateParticipantRatingRequest request,
        ClaimsPrincipal user,
        UpdateParticipantRatingCommandHandler handler,
        CancellationToken ct)
    {
        var userId = GetUserId(user);

        try
        {
            var result = await handler.HandleAsync(
                new UpdateParticipantRatingCommand(
                    watchSpaceId,
                    watchSpaceAnimeId,
                    userId,
                    request.RatingScore,
                    request.RatingNotes,
                    request.RatingNotesProvided),
                ct);

            return result is null
                ? Results.NotFound(new { error = "Anime not found in this watch space." })
                : Results.Ok(result);
        }
        catch (NotAWatchSpaceMemberException)
        {
            return Results.Forbid();
        }
        catch (AnimeTrackingDomainException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
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
