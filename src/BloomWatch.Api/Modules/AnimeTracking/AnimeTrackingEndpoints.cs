using System.Security.Claims;
using BloomWatch.Modules.AnimeTracking.Application.UseCases.AddAnimeToWatchSpace;
using BloomWatch.Modules.AnimeTracking.Application.UseCases.GetWatchSpaceAnimeDetail;
using BloomWatch.Modules.AnimeTracking.Application.UseCases.ListWatchSpaceAnime;
using BloomWatch.Modules.AnimeTracking.Application.UseCases.UpdateParticipantProgress;
using BloomWatch.Modules.AnimeTracking.Application.UseCases.RecordWatchSession;
using BloomWatch.Modules.AnimeTracking.Application.UseCases.UpdateParticipantRating;
using BloomWatch.Modules.AnimeTracking.Application.UseCases.UpdateSharedAnimeStatus;
using BloomWatch.Modules.AnimeTracking.Domain.Enums;
using BloomWatch.Modules.AnimeTracking.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace BloomWatch.Api.Modules.AnimeTracking;

/// <summary>
/// Defines the minimal API endpoints for the AnimeTracking module, covering anime list management,
/// shared status updates, individual participant progress and ratings, and watch session recording.
/// </summary>
public static class AnimeTrackingEndpoints
{
    /// <summary>
    /// Maps the AnimeTracking HTTP endpoints onto the application's routing pipeline.
    /// </summary>
    /// <remarks>
    /// <para>All endpoints are nested under <c>/watchspaces/{watchSpaceId}/anime</c> and require authorization.
    /// The caller must be a member of the specified watch space. Registers the following operations:</para>
    /// <list type="bullet">
    ///   <item><description>List and detail: browse tracked anime and view full details with participants and sessions.</description></item>
    ///   <item><description>Add anime: import an anime from the AniList cache into the watch space.</description></item>
    ///   <item><description>Shared status: update the group's collective tracking state (status, episodes, mood/vibe/pitch).</description></item>
    ///   <item><description>Individual progress: each member tracks their own status and episode count independently.</description></item>
    ///   <item><description>Ratings: each member submits a personal score and optional notes.</description></item>
    ///   <item><description>Watch sessions: log when and which episodes the group watched together.</description></item>
    /// </list>
    /// </remarks>
    /// <param name="app">The endpoint route builder to add the AnimeTracking routes to.</param>
    /// <returns>The same <see cref="IEndpointRouteBuilder"/> instance for chaining.</returns>
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

        group.MapPost("/{watchSpaceAnimeId:guid}/sessions", RecordWatchSessionAsync)
            .WithName("RecordWatchSession")
            .WithSummary("Record a watch session for an anime in a watch space")
            .WithDescription(
                "Creates a new watch session with an episode range and date for an anime " +
                "in the specified watch space. The caller must be a member of the watch space.")
            .Produces<RecordWatchSessionResult>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    /// <summary>
    /// Handles adding an anime to a watch space by its AniList media ID.
    /// </summary>
    /// <returns>
    /// A 201 Created result with the new tracked anime entry, a 403 Forbidden if the caller
    /// is not a member, a 409 Conflict if the anime is already tracked, or a 404 Not Found
    /// if the AniList media ID is not in the local cache.
    /// </returns>
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

    /// <summary>
    /// Handles listing all anime tracked in a watch space, with optional status filtering.
    /// </summary>
    /// <returns>
    /// A 200 OK result with the anime list, or a 403 Forbidden if the caller is not a member.
    /// </returns>
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

    /// <summary>
    /// Handles retrieving full detail for a single tracked anime including participants and watch sessions.
    /// </summary>
    /// <returns>
    /// A 200 OK result with the anime detail, a 404 Not Found if the anime is not tracked
    /// in this watch space, or a 403 Forbidden if the caller is not a member.
    /// </returns>
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

    /// <summary>
    /// Handles partial updates to the shared tracking state (status, episodes, mood/vibe/pitch) for an anime.
    /// </summary>
    /// <returns>
    /// A 200 OK result with the updated anime detail, a 400 Bad Request if the status value
    /// is invalid, a 404 Not Found if the anime is not tracked, or a 403 Forbidden if the caller
    /// is not a member.
    /// </returns>
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

    /// <summary>
    /// Handles updating the calling user's individual progress (status and episode count) for an anime.
    /// Creates the participant entry automatically if it does not already exist.
    /// </summary>
    /// <returns>
    /// A 200 OK result with the updated participant detail, a 400 Bad Request if the status value
    /// is invalid, a 404 Not Found if the anime is not tracked, or a 403 Forbidden if the caller
    /// is not a member.
    /// </returns>
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

    /// <summary>
    /// Handles submitting or updating the calling user's personal rating and optional notes for an anime.
    /// Creates the participant entry automatically if it does not already exist.
    /// </summary>
    /// <returns>
    /// A 200 OK result with the updated participant detail, a 400 Bad Request if the rating
    /// is invalid, a 404 Not Found if the anime is not tracked, or a 403 Forbidden if the caller
    /// is not a member.
    /// </returns>
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

    /// <summary>
    /// Handles recording a new watch session (date, episode range, optional notes) for an anime.
    /// </summary>
    /// <returns>
    /// A 201 Created result with the new session record, a 400 Bad Request if validation fails,
    /// a 404 Not Found if the anime is not tracked, or a 403 Forbidden if the caller is not a member.
    /// </returns>
    private static async Task<IResult> RecordWatchSessionAsync(
        Guid watchSpaceId,
        Guid watchSpaceAnimeId,
        [FromBody] RecordWatchSessionRequest request,
        ClaimsPrincipal user,
        RecordWatchSessionCommandHandler handler,
        CancellationToken ct)
    {
        var userId = GetUserId(user);

        try
        {
            var result = await handler.HandleAsync(
                new RecordWatchSessionCommand(
                    watchSpaceId,
                    watchSpaceAnimeId,
                    userId,
                    request.SessionDateUtc,
                    request.StartEpisode,
                    request.EndEpisode,
                    request.Notes),
                ct);

            return result is null
                ? Results.NotFound(new { error = "Anime not found in this watch space." })
                : Results.Created(
                    $"/watchspaces/{watchSpaceId}/anime/{watchSpaceAnimeId}/sessions/{result.Id}",
                    result);
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

    /// <summary>
    /// Extracts the user's unique identifier from the JWT claims principal.
    /// </summary>
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
/// <param name="AniListMediaId">The AniList media ID of the anime to add. Must exist in the local AniList cache (use <c>GET /api/anilist/search</c> first).</param>
/// <param name="Mood">An optional free-text mood tag describing the group's feeling about this pick (e.g., "cozy", "intense").</param>
/// <param name="Vibe">An optional free-text vibe tag for this anime (e.g., "late-night binge", "weekend chill").</param>
/// <param name="Pitch">An optional short pitch or reason why the group should watch this anime.</param>
public sealed record AddAnimeRequest(
    int AniListMediaId,
    string? Mood = null,
    string? Vibe = null,
    string? Pitch = null);
