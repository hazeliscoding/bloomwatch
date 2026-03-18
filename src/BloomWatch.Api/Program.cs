using BloomWatch.Api.Modules.AniListSync;
using BloomWatch.Api.Modules.Analytics;
using BloomWatch.Api.Modules.AnimeTracking;
using BloomWatch.Api.Modules.Identity;
using BloomWatch.Api.Modules.WatchSpaces;
using BloomWatch.Modules.AniListSync.Infrastructure.Extensions;
using BloomWatch.Modules.Analytics.Infrastructure.Extensions;
using BloomWatch.Modules.AnimeTracking.Infrastructure.Extensions;
using BloomWatch.Modules.Identity.Infrastructure.Extensions;
using BloomWatch.Modules.WatchSpaces.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddIdentityModule(builder.Configuration);
builder.Services.AddWatchSpacesModule(builder.Configuration);
builder.Services.AddAniListSyncModule(builder.Configuration);
builder.Services.AddAnimeTrackingModule(builder.Configuration);
builder.Services.AddAnalyticsModule(builder.Configuration);

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info = new OpenApiInfo
        {
            Title = "BloomWatch API",
            Version = "v1",
            Description = "BloomWatch modular monolith API — Identity, WatchSpaces, and AniListSync modules."
        };

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Enter your JWT access token in the field below."
        };

        return Task.CompletedTask;
    });

    options.AddOperationTransformer((operation, context, cancellationToken) =>
    {
        var requiresAuth = context.Description.ActionDescriptor.EndpointMetadata
            .Any(m => m is IAuthorizeData);

        if (requiresAuth)
        {
            operation.Security =
            [
                new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", context.Document)] = []
                }
            ];
        }

        return Task.CompletedTask;
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200") 
                  .AllowAnyHeader() // Allows all headers
                  .AllowAnyMethod(); // Allows all HTTP methods (GET, POST, PUT, DELETE, etc.)
        });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "BloomWatch API";
        options.DefaultHttpClient = new(ScalarTarget.Http, ScalarClient.HttpClient);
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseCors("AllowSpecificOrigin");


app.MapIdentityEndpoints();
app.MapWatchSpacesEndpoints();
app.MapAniListSyncEndpoints();
app.MapAnimeTrackingEndpoints();
app.MapAnalyticsEndpoints();

app.Run();

/// <summary>
/// The entry point class for the BloomWatch API application.
/// </summary>
/// <remarks>
/// Declared as a partial class to enable integration test projects (e.g., using
/// <c>WebApplicationFactory&lt;Program&gt;</c>) to reference the application entry point.
/// </remarks>
public partial class Program { }
