using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BloomWatch.Modules.Identity.Application.UseCases.ForgotPassword;
using BloomWatch.Modules.Identity.Application.UseCases.GetProfile;
using BloomWatch.Modules.Identity.Application.UseCases.Login;
using BloomWatch.Modules.Identity.Application.UseCases.RefreshToken;
using BloomWatch.Modules.Identity.Application.UseCases.Register;
using BloomWatch.Modules.Identity.Application.UseCases.ResetPassword;
using BloomWatch.Modules.Identity.Application.UseCases.RevokeToken;
using BloomWatch.Modules.Identity.Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BloomWatch.Api.Modules.Identity;

/// <summary>
/// Defines the minimal API endpoints for the Identity module, covering user registration,
/// authentication, and profile retrieval.
/// </summary>
public static class IdentityEndpoints
{
    /// <summary>
    /// Maps the Identity HTTP endpoints onto the application's routing pipeline.
    /// </summary>
    /// <remarks>
    /// <para>Registers the following endpoints:</para>
    /// <list type="bullet">
    ///   <item><description><c>POST /auth/register</c> -- Create a new user account.</description></item>
    ///   <item><description><c>POST /auth/login</c> -- Authenticate and receive a JWT access token.</description></item>
    ///   <item><description><c>GET /users/me</c> -- Retrieve the authenticated user's profile.</description></item>
    /// </list>
    /// </remarks>
    /// <param name="app">The endpoint route builder to add the Identity routes to.</param>
    /// <returns>The same <see cref="IEndpointRouteBuilder"/> instance for chaining.</returns>
    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth").WithTags("Identity");

        group.MapPost("/register", RegisterAsync)
            .WithName("Register")
            .WithSummary("Register a new user account")
            .WithDescription(
                "Creates a new user account with the provided email, password, and display name. " +
                "Returns the newly created user's ID on success.")
            .Produces<RegisterUserResult>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status409Conflict)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/login", LoginAsync)
            .WithName("Login")
            .WithSummary("Authenticate and obtain a JWT access token")
            .WithDescription(
                "Validates the provided credentials and, if correct, returns a signed JWT access token " +
                "along with its expiration timestamp.")
            .Produces<LoginUserResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost("/refresh", RefreshAsync)
            .WithName("RefreshToken")
            .WithSummary("Exchange a refresh token for a new token pair")
            .WithDescription("Rotates the refresh token and issues a new access + refresh token pair.")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost("/revoke", RevokeAsync)
            .WithName("RevokeToken")
            .WithSummary("Revoke a refresh token (logout)")
            .WithDescription("Invalidates the supplied refresh token. Idempotent for unknown tokens.")
            .Produces(StatusCodes.Status204NoContent);

        group.MapPost("/forgot-password", ForgotPasswordAsync)
            .WithName("ForgotPassword")
            .WithSummary("Request a password-reset email")
            .WithDescription(
                "Sends a password-reset link to the provided email address if it belongs to an active account. " +
                "Always returns 200 to prevent user enumeration. Rate-limited to 5 requests per email per hour.")
            .RequireRateLimiting("forgot-password-limit")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status429TooManyRequests);

        group.MapPost("/reset-password", ResetPasswordAsync)
            .WithName("ResetPassword")
            .WithSummary("Reset a user's password using a reset token")
            .WithDescription(
                "Validates the supplied reset token and, if valid, updates the user's password. " +
                "Returns 400 for expired, used, or invalid tokens.")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        var usersGroup = app.MapGroup("/users").WithTags("Users");

        usersGroup.MapGet("/me", GetMyProfileAsync)
            .WithName("GetMyProfile")
            .WithSummary("Get the authenticated user's profile")
            .WithDescription(
                "Returns the profile of the currently authenticated user, " +
                "including account status and email verification state.")
            .RequireAuthorization()
            .Produces<UserProfileResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    /// <summary>
    /// Handles user registration by creating a new account with the provided credentials.
    /// </summary>
    /// <param name="request">The registration request containing email, password, and display name.</param>
    /// <param name="handler">The command handler resolved from dependency injection.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>
    /// A 201 Created result with the new user's ID, a 409 Conflict if the email is already registered,
    /// or a 400 Bad Request if validation fails.
    /// </returns>
    private static async Task<IResult> RegisterAsync(
        [FromBody] RegisterRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new RegisterUserCommand(request.Email, request.Password, request.DisplayName);
            var result = await sender.Send(command, cancellationToken);
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

    /// <summary>
    /// Handles user login by validating credentials and issuing a JWT access token.
    /// </summary>
    /// <param name="request">The login request containing email and password.</param>
    /// <param name="handler">The command handler resolved from dependency injection.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>
    /// A 200 OK result with the access token and expiration, or a 401 Unauthorized result
    /// if credentials are invalid or the account is not active.
    /// </returns>
    private static async Task<IResult> LoginAsync(
        [FromBody] LoginRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new LoginUserCommand(request.Email, request.Password);
            var result = await sender.Send(command, cancellationToken);
            return Results.Ok(new
            {
                accessToken = result.AccessToken,
                expiresAt = result.ExpiresAt,
                refreshToken = result.RefreshToken,
                refreshTokenExpiresAt = result.RefreshTokenExpiresAt,
            });
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

    /// <summary>
    /// Retrieves the profile of the currently authenticated user from JWT claims.
    /// </summary>
    /// <param name="user">The authenticated user's claims principal from the JWT token.</param>
    /// <param name="handler">The query handler resolved from dependency injection.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>
    /// A 200 OK result with the user profile, a 401 Unauthorized if the user ID claim
    /// is missing or malformed, or a 404 Not Found if the user no longer exists.
    /// </returns>
    private static async Task<IResult> GetMyProfileAsync(
        ClaimsPrincipal user,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var subClaim = user.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (subClaim is null || !Guid.TryParse(subClaim, out var userId))
            return Results.Unauthorized();

        try
        {
            var query = new GetUserProfileQuery(UserId.From(userId));
            var result = await sender.Send(query, cancellationToken);
            return Results.Ok(result);
        }
        catch (UserNotFoundException)
        {
            return Results.NotFound();
        }
    }

    private static async Task<IResult> RefreshAsync(
        [FromBody] RefreshRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await sender.Send(new RefreshTokenCommand(request.RefreshToken), cancellationToken);
            return Results.Ok(new
            {
                accessToken = result.AccessToken,
                expiresAt = result.ExpiresAt,
                refreshToken = result.RefreshToken,
                refreshTokenExpiresAt = result.RefreshTokenExpiresAt,
            });
        }
        catch (InvalidRefreshTokenException)
        {
            return Results.Unauthorized();
        }
    }

    private static async Task<IResult> RevokeAsync(
        [FromBody] RevokeRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(new RevokeTokenCommand(request.RefreshToken), cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> ForgotPasswordAsync(
        [FromBody] ForgotPasswordRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(new ForgotPasswordCommand(request.Email), cancellationToken);
        return Results.Ok(new { message = "If that email is registered, a reset link has been sent." });
    }

    private static async Task<IResult> ResetPasswordAsync(
        [FromBody] ResetPasswordRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new ResetPasswordCommand(request.Token, request.NewPassword), cancellationToken);

        return result switch
        {
            ResetPasswordResult.Success =>
                Results.Ok(new { message = "Password has been reset successfully." }),
            ResetPasswordResult.TokenExpired =>
                Results.BadRequest(new { error = "Reset link has expired. Please request a new one." }),
            ResetPasswordResult.TokenAlreadyUsed =>
                Results.BadRequest(new { error = "Reset link has already been used. Please request a new one." }),
            ResetPasswordResult.WeakPassword =>
                Results.BadRequest(new { error = "Password must be at least 8 characters and include uppercase, lowercase, and a digit." }),
            _ => Results.BadRequest(new { error = "Invalid reset link. Please request a new one." }),
        };
    }
}

/// <summary>
/// Represents the request body for user registration.
/// </summary>
/// <param name="Email">The email address for the new account.</param>
/// <param name="Password">The password for the new account.</param>
/// <param name="DisplayName">The display name shown to other users.</param>
public sealed record RegisterRequest(string Email, string Password, string DisplayName);

/// <summary>
/// Represents the request body for user login.
/// </summary>
/// <param name="Email">The email address of the account to authenticate.</param>
/// <param name="Password">The password to validate against the account.</param>
public sealed record LoginRequest(string Email, string Password);

/// <summary>
/// Represents the request body for initiating a password reset.
/// </summary>
/// <param name="Email">The email address to send the reset link to.</param>
public sealed record ForgotPasswordRequest(string Email);

/// <summary>
/// Represents the request body for refreshing a token pair.
/// </summary>
/// <param name="RefreshToken">The current refresh token to exchange.</param>
public sealed record RefreshRequest(string RefreshToken);

/// <summary>
/// Represents the request body for revoking a refresh token.
/// </summary>
/// <param name="RefreshToken">The refresh token to revoke.</param>
public sealed record RevokeRequest(string RefreshToken);

/// <summary>
/// Represents the request body for completing a password reset.
/// </summary>
/// <param name="Token">The plain reset token from the email link.</param>
/// <param name="NewPassword">The new password to set.</param>
public sealed record ResetPasswordRequest(string Token, string NewPassword);
