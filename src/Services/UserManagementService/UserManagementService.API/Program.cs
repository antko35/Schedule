using System.Reflection;
using Infrastructure.Extensions;
using Microsoft.OpenApi.Models;
using UserManagementService.API.Extensions;
using UserManagementService.API.Extensions.AppExtensions;
using UserManagementService.API.Extensions.AppExtensions.Hangfire;
using UserManagementService.API.Keycloak;
using UserManagementService.Application.Extensions;
using UserManagementService.Data.Extensions;
using UserManagementService.DataAccess.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddMongoDBClient("userManagementDb");

builder.AddRabbitMQClient("rabbitmq");

// Add services to the container.
builder.Services.AddControllers();

// Keycloak Admin API client
builder.Services.AddHttpClient(
    "keycloak-admin",
    client =>
{
    client.BaseAddress = new Uri("http://keycloak");
}).AddServiceDiscovery();

builder.Services.AddScoped<IKeycloakAdminService, KeycloakAdminService>();

// Application Layer
builder.Services
    .AddApplicatiobLayerDependencis();

// DataAccess Layer
builder.Services
    .ConfigureDb(builder.Configuration)
    .ConfigureHangfire()
    .AddDataLayerDependencis();

builder.Services
    .AddInfrastructureDependencies();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.SetIsOriginAllowed(_ => builder.Environment.IsDevelopment())
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var keycloakPublicUrl =
    builder.Configuration["Keycloak:PublicUrl"]
    ?? throw new InvalidOperationException("Keycloak:PublicUrl configuration is required.");
const string realmName = "schedule-service";
const string securitySchemeId = "oauth2";

builder.Services.AddAuthentication()
    .AddKeycloakJwtBearer(
        serviceName: "keycloak",
        realm: realmName,
        configureOptions: options =>
        {
            options.Audience = realmName;
            options.TokenValidationParameters.ValidIssuer =
                $"{keycloakPublicUrl.TrimEnd('/')}/realms/{realmName}";

            if (builder.Environment.IsDevelopment())
            {
                options.RequireHttpsMetadata = false;
            }
        });

builder.Services.AddAuthorizationBuilder();

builder.Services.AddSwaggerGen(options =>
{
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

    var authUrl = new Uri($"{keycloakPublicUrl.TrimEnd('/')}/realms/{realmName}/protocol/openid-connect/auth");
    var tokenUrl = new Uri($"{keycloakPublicUrl.TrimEnd('/')}/realms/{realmName}/protocol/openid-connect/token");

    options.AddSecurityDefinition(
        securitySchemeId,
        new OpenApiSecurityScheme
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
                    Id = securitySchemeId,
                },
            },
            new[] { "openid", "profile" }
        },
    });
});

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseMiddleware<ExeptionHadlingMiddleware>();

app.ConfigureSwagger();

app.ConfigureHangfire();

app.UseCors();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
