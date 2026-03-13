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

public static class ServiceCollectionExtensions
{
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
