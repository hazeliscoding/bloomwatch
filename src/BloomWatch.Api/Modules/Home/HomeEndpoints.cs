using System.Security.Claims;

namespace BloomWatch.Api.Modules.Home;

/// <summary>
/// Defines the minimal API endpoint for the Home overview, aggregating data
/// from Identity, WatchSpaces, and AnimeTracking modules.
/// </summary>
public static class HomeEndpoints
{
    /// <summary>
    /// Maps the Home HTTP endpoints onto the application's routing pipeline.
    /// </summary>
    public static IEndpointRouteBuilder MapHomeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/home").WithTags("Home").RequireAuthorization();

        group.MapGet("/overview", GetOverviewAsync)
            .WithName("GetHomeOverview")
            .WithSummary("Get the authenticated user's home overview")
            .WithDescription(
                "Returns an aggregated overview including the user's display name, " +
                "global stats across all watch spaces, watch space summaries with per-space counts, " +
                "and the 3 most recently updated anime.")
            .Produces<HomeOverviewResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        return app;
    }

    private static async Task<IResult> GetOverviewAsync(
        ClaimsPrincipal user,
        GetHomeOverviewQueryHandler handler,
        CancellationToken ct)
    {
        var userId = GetUserId(user);
        var result = await handler.HandleAsync(new GetHomeOverviewQuery(userId), ct);
        return Results.Ok(result);
    }

    private static Guid GetUserId(ClaimsPrincipal user)
    {
        var sub = user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue("sub")
            ?? throw new InvalidOperationException("User ID claim not found.");
        return Guid.Parse(sub);
    }
}
