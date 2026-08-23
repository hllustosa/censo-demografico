using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Census.Shared.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Census.Shared.Web;

public static class AuthExtensions
{
    public static IServiceCollection AddCensusAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
                    RoleClaimType = ClaimTypes.Role
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(CensusPolicies.CanManagePeople, policy =>
                policy.RequireRole(CensusRoles.Registrar, CensusRoles.Admin));

            options.AddPolicy(CensusPolicies.CanReadPeople, policy =>
                policy.RequireRole(CensusRoles.Registrar, CensusRoles.Analyst, CensusRoles.Admin));

            options.AddPolicy(CensusPolicies.CanViewDashboard, policy =>
                policy.RequireRole(CensusRoles.Analyst, CensusRoles.Admin));

            options.AddPolicy(CensusPolicies.CanViewFamilyTree, policy =>
                policy.RequireRole(CensusRoles.Registrar, CensusRoles.Analyst, CensusRoles.Admin));

            options.AddPolicy(CensusPolicies.CanManageUsers, policy =>
                policy.RequireRole(CensusRoles.Admin));
        });

        return services;
    }

    public static string GenerateAccessToken(JwtOptions options, IEnumerable<Claim> claims)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: options.Issuer,
            audience: options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(options.AccessTokenMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
