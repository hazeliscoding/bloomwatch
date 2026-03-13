using BloomWatch.Modules.Identity.Application.UseCases.Login;
using BloomWatch.Modules.Identity.Application.UseCases.Register;
using Microsoft.AspNetCore.Mvc;

namespace BloomWatch.Api.Modules.Identity;

public static class IdentityEndpoints
{
    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth").WithTags("Identity");

        group.MapPost("/register", RegisterAsync);
        group.MapPost("/login", LoginAsync);

        return app;
    }

    private static async Task<IResult> RegisterAsync(
        [FromBody] RegisterRequest request,
        RegisterUserCommandHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new RegisterUserCommand(request.Email, request.Password, request.DisplayName);
            var result = await handler.HandleAsync(command, cancellationToken);
            return Results.Created($"/users/{result.UserId}", result);
        }
        catch (DuplicateEmailException ex)
        {
            return Results.Conflict(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (RegistrationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> LoginAsync(
        [FromBody] LoginRequest request,
        LoginUserCommandHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new LoginUserCommand(request.Email, request.Password);
            var result = await handler.HandleAsync(command, cancellationToken);
            return Results.Ok(new { accessToken = result.AccessToken, expiresAt = result.ExpiresAt });
        }
        catch (InvalidCredentialsException)
        {
            return Results.Unauthorized();
        }
        catch (AccountNotActiveException)
        {
            return Results.Unauthorized();
        }
    }
}

public sealed record RegisterRequest(string Email, string Password, string DisplayName);
public sealed record LoginRequest(string Email, string Password);
