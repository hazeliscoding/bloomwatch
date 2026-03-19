using System.Security.Claims;
using BloomWatch.Modules.Analytics.Application.Exceptions;
using BloomWatch.Modules.Analytics.Application.UseCases.GetCompatibility;
using BloomWatch.Modules.Analytics.Application.UseCases.GetDashboardSummary;
using BloomWatch.Modules.Analytics.Application.UseCases.GetRatingGaps;

namespace BloomWatch.Api.Modules.Analytics;

public static class AnalyticsEndpoints
{
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

        return app;
    }

    private static async Task<IResult> GetDashboardSummaryAsync(
        Guid watchSpaceId,
        ClaimsPrincipal user,
        GetDashboardSummaryQueryHandler handler,
        CancellationToken ct)
    {
        var userId = GetUserId(user);

        try
        {
            var result = await handler.HandleAsync(
                new GetDashboardSummaryQuery(watchSpaceId, userId), ct);

            return Results.Ok(result);
        }
        catch (NotAWatchSpaceMemberException)
        {
            return Results.Forbid();
        }
    }

    private static async Task<IResult> GetCompatibilityAsync(
        Guid watchSpaceId,
        ClaimsPrincipal user,
        GetCompatibilityQueryHandler handler,
        CancellationToken ct)
    {
        var userId = GetUserId(user);

        try
        {
            var result = await handler.HandleAsync(
                new GetCompatibilityQuery(watchSpaceId, userId), ct);

            return Results.Ok(result);
        }
        catch (NotAWatchSpaceMemberException)
        {
            return Results.Forbid();
        }
    }

    private static async Task<IResult> GetRatingGapsAsync(
        Guid watchSpaceId,
        ClaimsPrincipal user,
        GetRatingGapsQueryHandler handler,
        CancellationToken ct)
    {
        var userId = GetUserId(user);

        try
        {
            var result = await handler.HandleAsync(
                new GetRatingGapsQuery(watchSpaceId, userId), ct);

            return Results.Ok(result);
        }
        catch (NotAWatchSpaceMemberException)
        {
            return Results.Forbid();
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
