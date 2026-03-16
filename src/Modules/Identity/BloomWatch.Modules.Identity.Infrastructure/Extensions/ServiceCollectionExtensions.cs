using System.Text;
using BloomWatch.Modules.Identity.Application.Abstractions;
using BloomWatch.Modules.Identity.Application.UseCases.GetProfile;
using BloomWatch.Modules.Identity.Application.UseCases.Login;
using BloomWatch.Modules.Identity.Application.UseCases.Register;
using BloomWatch.Modules.Identity.Domain.Repositories;
using BloomWatch.Modules.Identity.Infrastructure.Auth;
using BloomWatch.Modules.Identity.Infrastructure.Persistence;
using BloomWatch.Modules.Identity.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace BloomWatch.Modules.Identity.Infrastructure.Extensions;

/// <summary>
/// Provides extension methods for registering the Identity module's services
/// with the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all Identity module services, including persistence, authentication,
    /// authorization, and application-layer use-case handlers.
    /// </summary>
    /// <remarks>
    /// <para>The following services are registered:</para>
    /// <list type="bullet">
    ///   <item><description><b>Persistence</b> -- <see cref="IdentityDbContext"/> backed by PostgreSQL (connection string <c>DefaultConnection</c>).</description></item>
    ///   <item><description><b>Repositories</b> -- <see cref="IUserRepository"/> as scoped.</description></item>
    ///   <item><description><b>Auth services</b> -- <see cref="IPasswordHasher"/> (bcrypt) and <see cref="IJwtTokenGenerator"/> as scoped.</description></item>
    ///   <item><description><b>Use-case handlers</b> -- <see cref="RegisterUserCommandHandler"/>, <see cref="LoginUserCommandHandler"/>, and <see cref="GetUserProfileQueryHandler"/> as scoped.</description></item>
    ///   <item><description><b>JWT bearer authentication</b> -- configured with HMAC-SHA256 validation, zero clock skew, and issuer/audience checks sourced from <c>Identity:Jwt:*</c> configuration.</description></item>
    ///   <item><description><b>Authorization</b> -- default policy via <c>AddAuthorization()</c>.</description></item>
    /// </list>
    /// </remarks>
    /// <param name="services">The service collection to register services into.</param>
    /// <param name="configuration">
    /// The application configuration. Must contain a <c>ConnectionStrings:DefaultConnection</c> entry
    /// and an <c>Identity:Jwt:SecretKey</c> entry. <c>Identity:Jwt:Issuer</c> and
    /// <c>Identity:Jwt:Audience</c> are optional (default to <c>"BloomWatch"</c>).
    /// </param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the required <c>Identity:Jwt:SecretKey</c> configuration value is missing.
    /// </exception>
    public static IServiceCollection AddIdentityModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Persistence
        services.AddDbContext<IdentityDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Repositories
        services.AddScoped<IUserRepository, EfUserRepository>();

        // Auth services
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        // Use case handlers
        services.AddScoped<RegisterUserCommandHandler>();
        services.AddScoped<LoginUserCommandHandler>();
        services.AddScoped<GetUserProfileQueryHandler>();

        // JWT bearer authentication
        var secretKey = configuration["Identity:Jwt:SecretKey"]
            ?? throw new InvalidOperationException("JWT secret key is not configured (Identity:Jwt:SecretKey).");
        var issuer = configuration["Identity:Jwt:Issuer"] ?? "BloomWatch";
        var audience = configuration["Identity:Jwt:Audience"] ?? "BloomWatch";

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization();

        return services;
    }
}
