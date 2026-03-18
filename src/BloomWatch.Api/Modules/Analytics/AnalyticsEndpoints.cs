using System.Security.Claims;
using BloomWatch.Modules.Analytics.Application.Exceptions;
using BloomWatch.Modules.Analytics.Application.UseCases.GetDashboardSummary;

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

    private static Guid GetUserId(ClaimsPrincipal user)
    {
        var sub = user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue("sub")
            ?? throw new InvalidOperationException("User ID claim not found.");
        return Guid.Parse(sub);
    }
}
