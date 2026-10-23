using System.Reflection;
using Microsoft.OpenApi.Models;

namespace ScheduleService.API.Extensions;

public static class SwaggerExtension
{
    private const string RealmName = "schedule-service";
    private const string SecuritySchemeId = "oauth2";

    public static IServiceCollection AddSwaggerExtension(this IServiceCollection services, string keycloakBaseUrl)
    {
        services.AddSwaggerGen(options =>
        {
            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

            var authUrl = new Uri($"{keycloakBaseUrl.TrimEnd('/')}/realms/{RealmName}/protocol/openid-connect/auth");
            var tokenUrl = new Uri($"{keycloakBaseUrl.TrimEnd('/')}/realms/{RealmName}/protocol/openid-connect/token");

            options.AddSecurityDefinition(SecuritySchemeId, new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = authUrl,
                        TokenUrl = tokenUrl,
                        Scopes = new Dictionary<string, string>
                        {
                            { "openid", "OpenID identity" },
                            { "profile", "User profile" },
                        },
                    },
                },
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = SecuritySchemeId,
                        },
                    },
                    new[] { "openid", "profile" }
                },
            });
        });

        return services;
    }
}
