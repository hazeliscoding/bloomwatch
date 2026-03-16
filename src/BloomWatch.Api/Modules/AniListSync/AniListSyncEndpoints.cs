using BloomWatch.Modules.AniListSync.Application.UseCases.SearchAnime;
using BloomWatch.Modules.AniListSync.Infrastructure.AniList;

namespace BloomWatch.Api.Modules.AniListSync;

public static class AniListSyncEndpoints
{
    public static IEndpointRouteBuilder MapAniListSyncEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/anilist").WithTags("AniList");

        group.MapGet("/search", SearchAnimeAsync)
            .WithName("SearchAnime")
            .WithSummary("Search for anime via AniList")
            .WithDescription(
                "Proxies search queries to the AniList GraphQL API and returns matching anime results. " +
                "Results are cached in memory for 5 minutes.")
            .RequireAuthorization()
            .Produces<IReadOnlyList<AnimeSearchResult>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status502BadGateway);

        return app;
    }

    private static async Task<IResult> SearchAnimeAsync(
        string? query,
        SearchAnimeQueryHandler handler,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Results.BadRequest(new { error = "The 'query' parameter is required and must not be empty." });

        try
        {
            var searchQuery = new SearchAnimeQuery(query);
            var results = await handler.HandleAsync(searchQuery, cancellationToken);
            return Results.Ok(results);
        }
        catch (AniListApiException ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status502BadGateway,
                title: "AniList API Error");
        }
    }
}
