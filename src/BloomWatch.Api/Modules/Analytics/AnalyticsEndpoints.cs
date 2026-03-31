using System.Security.Claims;
using BloomWatch.Modules.Analytics.Application.Exceptions;
using BloomWatch.Modules.Analytics.Application.UseCases.GetCompatibility;
using BloomWatch.Modules.Analytics.Application.UseCases.GetDashboardSummary;
using BloomWatch.Modules.Analytics.Application.UseCases.GetRatingGaps;
using BloomWatch.Modules.Analytics.Application.UseCases.GetRandomPick;
using BloomWatch.Modules.Analytics.Application.UseCases.GetSharedStats;
using MediatR;

namespace BloomWatch.Api.Modules.Analytics;

/// <summary>
/// Defines the minimal API endpoints for the Analytics module, providing aggregated insights
/// about a watch space's activity, compatibility, rating differences, and random picks.
/// </summary>
public static class AnalyticsEndpoints
{
    /// <summary>
    /// Maps the Analytics HTTP endpoints onto the application's routing pipeline.
    /// </summary>
    /// <remarks>
    /// <para>All endpoints are nested under <c>/watchspaces/{watchSpaceId}</c> and require authorization.
    /// The caller must be a member of the specified watch space. Registers the following analytics queries:</para>
    /// <list type="bullet">
    ///   <item><description>Dashboard: a single-call composite view combining stats, currently watching, backlog highlights, rating gaps, and compatibility.</description></item>
    ///   <item><description>Compatibility: a score derived from shared anime ratings indicating how closely members' tastes align.</description></item>
    ///   <item><description>Rating gaps: anime where members have the largest divergence in personal ratings.</description></item>
    ///   <item><description>Shared stats: aggregate counters for episodes watched together, finished/dropped shows, and session counts.</description></item>
    ///   <item><description>Random pick: selects a random anime from the backlog to help the group decide what to watch next.</description></item>
    /// </list>
    /// </remarks>
    /// <param name="app">The endpoint route builder to add the Analytics routes to.</param>
    /// <returns>The same <see cref="IEndpointRouteBuilder"/> instance for chaining.</returns>
    public static IEndpointRouteBuilder MapAnalyticsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/watchspaces/{watchSpaceId:guid}")
            .WithTags("Analytics")
            .RequireAuthorization();

        group.MapGet("/dashboard", GetDashboardSummaryAsync)
            .WithName("GetDashboardSummary")
            .WithSummary("Get the dashboard summary for a watch space")
            .WithDescription(
                "Returns an aggregated dashboard overview including stats, currently-watching list, " +
                "backlog highlights, rating-gap highlights, and compatibility score. " +
                "The caller must be a member of the watch space.")
            .Produces<DashboardSummaryResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/analytics/compatibility", GetCompatibilityAsync)
            .WithName("GetCompatibility")
            .WithSummary("Get the compatibility score for a watch space")
            .WithDescription(
                "Returns the compatibility score computed from members' anime ratings. " +
                "The caller must be a member of the watch space.")
            .Produces<CompatibilityScoreResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapGet("/analytics/rating-gaps", GetRatingGapsAsync)
            .WithName("GetRatingGaps")
            .WithSummary("Get all rating gaps for a watch space")
            .WithDescription(
                "Returns all anime where at least 2 members have submitted ratings, " +
                "sorted by descending gap magnitude with alphabetical title tie-breaking. " +
                "The caller must be a member of the watch space.")
            .Produces<RatingGapsResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapGet("/analytics/shared-stats", GetSharedStatsAsync)
            .WithName("GetSharedStats")
            .WithSummary("Get shared watch statistics for a watch space")
            .WithDescription(
                "Returns aggregate statistics about the watch space's shared watch history " +
                "including total episodes, finished/dropped counts, and session activity. " +
                "The caller must be a member of the watch space.")
            .Produces<SharedStatsResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapGet("/analytics/random-pick", GetRandomPickAsync)
            .WithName("GetRandomPick")
            .WithSummary("Pick a random anime from the watch space backlog")
            .WithDescription(
                "Selects one anime at random from the backlog. " +
                "Returns null with a message if the backlog is empty. " +
                "The caller must be a member of the watch space.")
            .Produces<RandomPickResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden);

        return app;
    }

    /// <summary>
    /// Handles retrieving the composite dashboard summary for a watch space, combining stats,
    /// currently-watching list, backlog highlights, rating gaps, and compatibility in a single call.
    /// </summary>
    private static async Task<IResult> GetDashboardSummaryAsync(
        Guid watchSpaceId,
        ClaimsPrincipal user,
        ISender sender,
        CancellationToken ct)
    {
        var userId = GetUserId(user);

        try
        {
            var result = await sender.Send(
                new GetDashboardSummaryQuery(watchSpaceId, userId), ct);

            return Results.Ok(result);
        }
        catch (NotAWatchSpaceMemberException)
        {
            return Results.Forbid();
        }
    }

    /// <summary>
    /// Handles computing and returning the taste-compatibility score for a watch space.
    /// Requires at least two members with shared rated anime to produce a score.
    /// </summary>
    private static async Task<IResult> GetCompatibilityAsync(
        Guid watchSpaceId,
        ClaimsPrincipal user,
        ISender sender,
        CancellationToken ct)
    {
        var userId = GetUserId(user);

        try
        {
            var result = await sender.Send(
                new GetCompatibilityQuery(watchSpaceId, userId), ct);

            return Results.Ok(result);
        }
        catch (NotAWatchSpaceMemberException)
        {
            return Results.Forbid();
        }
    }

    /// <summary>
    /// Handles listing all anime with rating divergence between members, sorted by descending gap
    /// magnitude with alphabetical title tie-breaking.
    /// </summary>
    private static async Task<IResult> GetRatingGapsAsync(
        Guid watchSpaceId,
        ClaimsPrincipal user,
        ISender sender,
        CancellationToken ct)
    {
        var userId = GetUserId(user);

        try
        {
            var result = await sender.Send(
                new GetRatingGapsQuery(watchSpaceId, userId), ct);

            return Results.Ok(result);
        }
        catch (NotAWatchSpaceMemberException)
        {
            return Results.Forbid();
        }
    }

    /// <summary>
    /// Handles returning aggregate statistics about the watch space's shared activity,
    /// including total episodes watched together, finished/dropped counts, and session totals.
    /// </summary>
    private static async Task<IResult> GetSharedStatsAsync(
        Guid watchSpaceId,
        ClaimsPrincipal user,
        ISender sender,
        CancellationToken ct)
    {
        var userId = GetUserId(user);

        try
        {
            var result = await sender.Send(
                new GetSharedStatsQuery(watchSpaceId, userId), ct);

            return Results.Ok(result);
        }
        catch (NotAWatchSpaceMemberException)
        {
            return Results.Forbid();
        }
    }

    /// <summary>
    /// Handles selecting a random anime from the watch space's backlog.
    /// Returns a null pick with a descriptive message if the backlog is empty.
    /// </summary>
    private static async Task<IResult> GetRandomPickAsync(
        Guid watchSpaceId,
        ClaimsPrincipal user,
        ISender sender,
        CancellationToken ct)
    {
        var userId = GetUserId(user);

        try
        {
            var result = await sender.Send(
                new GetRandomPickQuery(watchSpaceId, userId), ct);

            return Results.Ok(result);
        }
        catch (NotAWatchSpaceMemberException)
        {
            return Results.Forbid();
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
