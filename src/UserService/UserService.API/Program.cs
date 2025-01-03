using UserService.API.Extensions;
using UserService.Application.Services;
using UserService.Domain.Models;
using UserService.Infrastructure.Extensions;
using UserService.Infrastructure.Seeders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8080, opt =>
    {
        var certPath = Environment.GetEnvironmentVariable("ASPNETCORE_Kestrel__Certificates__Default__Path");
        var certPassword = Environment.GetEnvironmentVariable("ASPNETCORE_Kestrel__Certificates__Default__Password");

        if (!string.IsNullOrEmpty(certPath) && !string.IsNullOrEmpty(certPassword))
        {
            opt.UseHttps(certPath, certPassword);
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin();
        builder.AllowAnyMethod();
        builder.AllowAnyHeader();
    });
});

builder.AddPresentation();

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<ExeptionHadlingMiddleware>();

var scope = app.Services.CreateScope();
var seeder = scope.ServiceProvider.GetRequiredService<IUserSeeder>();
await seeder.Seed();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();

    app.UseSwagger();
    app.UseSwaggerUI(options => // UseSwaggerUI is called only in Development.
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = string.Empty;
    });

    app.ApplyMigrations();
}

app.UseCors();

app.UseHttpsRedirection();

app.MapGrpcService<GrpcService>();

app.MapGroup("userService").MapIdentityApi<User>();

app.UseAuthorization();

app.MapControllers();

app.Run();
