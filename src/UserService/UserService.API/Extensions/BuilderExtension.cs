using Microsoft.OpenApi.Models;

namespace UserService.API.Extensions;

public static class BuilderExtension
{
    public static void AddPresentation(this WebApplicationBuilder builder) {

        builder.Services.AddAuthorization();
        builder.Services.AddControllers();

        builder.Services.AddSwaggerGen(c =>
        {
            c.AddSecurityDefinition("bearerAuth", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement // добавляет токен в header
            {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "bearerAuth" }
            },
            []
        }
    });
        });

        builder.Services.AddEndpointsApiExplorer(); // visible identity endpoints

        // exeption handling middleware

    }
}
