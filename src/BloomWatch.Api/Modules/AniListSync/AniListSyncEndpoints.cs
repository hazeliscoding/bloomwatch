using BloomWatch.Modules.AniListSync.Application.UseCases.GetMediaDetail;
using BloomWatch.Modules.AniListSync.Application.UseCases.SearchAnime;
using BloomWatch.Modules.AniListSync.Infrastructure.AniList;

namespace BloomWatch.Api.Modules.AniListSync;

/// <summary>
/// Defines the minimal API endpoints for the AniListSync module.
/// </summary>
public static class AniListSyncEndpoints
{
    /// <summary>
    /// Maps the AniListSync HTTP endpoints onto the application's routing pipeline.
    /// </summary>
    /// <remarks>
    /// Registers a <c>GET /api/anilist/search</c> endpoint that proxies search queries
    /// to the AniList GraphQL API. The endpoint requires authorization and returns
    /// cached results when available.
    /// </remarks>
    /// <param name="app">The endpoint route builder to add the AniList routes to.</param>
    /// <returns>The same <see cref="IEndpointRouteBuilder"/> instance for chaining.</returns>
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

        group.MapGet("/media/{anilistMediaId:int}", GetMediaDetailAsync)
            .WithName("GetMediaDetail")
            .WithSummary("Get full metadata for a single AniList anime")
            .WithDescription(
                "Returns full cached metadata for the given AniList media ID. " +
                "Fetches from AniList on cache miss or stale cache (24-hour freshness window).")
            .RequireAuthorization()
            .Produces<AnimeMediaDetail>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status502BadGateway);

        return app;
    }

    /// <summary>
    /// Handles the anime search request by validating the query parameter and delegating
    /// to the <see cref="SearchAnimeQueryHandler"/>.
    /// </summary>
    /// <param name="query">The search term provided as a query string parameter.</param>
    /// <param name="handler">The query handler resolved from dependency injection.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>
    /// An <see cref="IResult"/> containing a 200 OK with the search results,
    /// a 400 Bad Request if the query is empty, or a 502 Bad Gateway if the AniList API fails.
    /// </returns>
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

    /// <summary>
    /// Handles the media detail request by delegating to the <see cref="GetMediaDetailQueryHandler"/>.
    /// </summary>
    private static async Task<IResult> GetMediaDetailAsync(
        int anilistMediaId,
        GetMediaDetailQueryHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetMediaDetailQuery(anilistMediaId);
            var result = await handler.HandleAsync(query, cancellationToken);

            return result is null
                ? Results.NotFound()
                : Results.Ok(result);
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
