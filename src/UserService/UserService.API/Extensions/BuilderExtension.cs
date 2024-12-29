namespace UserService.API.Extensions;

using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;
using UserService.Domain.Models;
using UserService.Infrastructure;

public static class BuilderExtension
{
    public static void AddPresentation(this WebApplicationBuilder builder) {

        builder.Services.AddAuthentication();
        builder.Services.AddControllers();

        builder.Services.AddIdentityApiEndpoints<User>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<UserDbContext>();

        builder.Services.AddSwaggerGen(c =>
        {
            c.AddSecurityDefinition("bearerAuth", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement // добавляет токен в header
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "bearerAuth" },
                    },
                    []
                },
            });
        });

        builder.Services.AddEndpointsApiExplorer(); // visible identity endpoints
        builder.Services.AddScoped<Application.Services.UserService>();
        // exeption handling middleware
    }
}
