using ScheduleService.API.Extensions;
using ScheduleService.API.Extensions.AppExtensions.Hangfire;
using ScheduleService.Application.Extensions;
using ScheduleService.DataAccess.Extensions;
using ScheduleService.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddMongoDBClient("schedule-db");

builder.AddRabbitMQClient("rabbitmq");

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddApplicatiobLayerDependencis();

builder.Services
    .ConfigureDb(builder.Configuration)
    .HangfireConfigure()
    .AddDataAccessDependencis();

builder.Services
    .AddInfrastructureDependencis(builder.Configuration);

var keycloakPublicUrl =
    builder.Configuration["Keycloak:PublicUrl"]
    ?? throw new InvalidOperationException("Keycloak:PublicUrl configuration is required.");
builder.Services
    .AddSwaggerExtension(keycloakPublicUrl);

builder.Services.AddAuthentication()
    .AddKeycloakJwtBearer(
        serviceName: "keycloak",
        realm: "schedule-service",
        configureOptions: options =>
        {
            options.Audience = "schedule-service";
            options.TokenValidationParameters.ValidIssuer =
                $"{keycloakPublicUrl.TrimEnd('/')}/realms/schedule-service";

            if (builder.Environment.IsDevelopment())
            {
                options.RequireHttpsMetadata = false;
            }
        });

builder.Services.AddAuthorizationBuilder();

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseMiddleware<ExeptionHadlingMiddleware>();

app.ConfigureHangfire();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = string.Empty;
        options.OAuthClientId("schedule-service");
        options.OAuthUsePkce();
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
