using BloomWatch.Api.Modules.Identity;
using BloomWatch.Modules.Identity.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddIdentityModule(builder.Configuration);

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapIdentityEndpoints();

app.Run();

public partial class Program { }
