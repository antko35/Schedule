namespace UserService.API.Extensions;

using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;
using UserService.Domain.Models;
using UserService.Infrastructure;
using UserService.Infrastructure.EmailSender;

public static class BuilderExtension
{
    public static void AddPresentation(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthentication();
        builder.Services.AddControllers();

        builder.Services.AddGrpc();

        builder.Services.AddIdentityApiEndpoints<User>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<UserDbContext>();

        builder.Services.Configure<IdentityOptions>(options =>
        {
            options.User.RequireUniqueEmail = true; // Включение проверки уникальности Email
        });

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

        // builder.Services.AddAuthentication(options =>
        // {
        //    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        //    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        // }).AddJwtBearer(options =>
        // {
        //    options.TokenValidationParameters = new TokenValidationParameters
        //    {
        //        ValidateIssuer = true,
        //        ValidateAudience = true,
        //        ValidateLifetime = true,
        //        ValidateIssuerSigningKey = true,
        //        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        //        ValidAudience = builder.Configuration["Jwt:Audience"],
        //        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        //    };
        // })
        // .AddCookie(options =>
        // {
        //    options.LoginPath = "/login"; // Путь для перенаправления при неаутентифицированном доступе
        //    options.Cookie.Name = "authCookie"; // Имя куки
        //    options.Cookie.HttpOnly = true; // Защита от доступа через JavaScript
        //    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Использование только по HTTPS
        // });;
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddScoped<Application.Services.UserService>();
    }
}
