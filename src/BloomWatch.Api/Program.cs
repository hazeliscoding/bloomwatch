using BloomWatch.Api.Modules.Identity;
using BloomWatch.Api.Modules.WatchSpaces;
using BloomWatch.Modules.Identity.Infrastructure.Extensions;
using BloomWatch.Modules.WatchSpaces.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddIdentityModule(builder.Configuration);
builder.Services.AddWatchSpacesModule(builder.Configuration);

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapIdentityEndpoints();
app.MapWatchSpacesEndpoints();

app.Run();

public partial class Program { }
