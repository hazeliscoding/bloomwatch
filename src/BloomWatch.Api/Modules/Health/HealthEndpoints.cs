using BloomWatch.Modules.Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BloomWatch.Api.Modules.Health;

/// <summary>
/// Defines the health-check endpoint. Publicly accessible — no authentication required.
/// </summary>
public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/health", CheckHealthAsync)
            .WithTags("Health")
            .WithName("GetHealth")
            .WithSummary("Service health check")
            .WithDescription(
                "Returns the liveness status of the API and its database connection. " +
                "Returns 200 when healthy, 503 when degraded. No authentication required.")
            .Produces<HealthResponse>(StatusCodes.Status200OK)
            .Produces<HealthResponse>(StatusCodes.Status503ServiceUnavailable)
            .AllowAnonymous();

        return app;
    }

    private static async Task<IResult> CheckHealthAsync(
        IdentityDbContext db,
        CancellationToken ct)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromSeconds(2));

        try
        {
            await db.Database.ExecuteSqlRawAsync("SELECT 1", cts.Token);
            var response = new HealthResponse("ok", "ok");
            return Results.Ok(response);
        }
        catch
        {
            var response = new HealthResponse("degraded", "degraded");
            return Results.Json(response, statusCode: StatusCodes.Status503ServiceUnavailable);
        }
    }

    private sealed record HealthResponse(string Status, string Db);
}
