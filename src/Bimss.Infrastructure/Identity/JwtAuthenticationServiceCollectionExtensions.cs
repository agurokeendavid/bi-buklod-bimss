using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Bimss.Infrastructure.Identity;

// Own top-level call, not folded into AddBimssInfrastructure — only
// Bimss.Api uses JWT bearer as its default authentication scheme.
// Bimss.Web keeps AddBimssIdentity's cookie scheme untouched.
public static class JwtAuthenticationServiceCollectionExtensions
{
    public static IServiceCollection AddBimssJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddScoped<JwtTokenService>();

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer();

        // Configured via the DI-aware IOptions<JwtOptions> overload rather
        // than reading the signing key eagerly out of `configuration` here:
        // WebApplicationFactory-based tests layer additional config sources
        // onto the host after this extension method already ran during
        // Program.cs's synchronous startup, so an eagerly-captured value
        // would miss those overrides. Resolving JwtOptions lazily (at the
        // point JwtBearerOptions is actually created, well after the host
        // finishes building) picks up the final merged configuration.
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((bearerOptions, jwtOptions) =>
            {
                bearerOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Value.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Value.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Value.SigningKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                };
            });

        return services;
    }
}
