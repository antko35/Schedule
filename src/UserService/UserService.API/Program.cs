using UserService.API.Extensions;
using UserService.Application.Services;
using UserService.Domain.Models;
using UserService.Infrastructure.Extensions;
using UserService.Infrastructure.Seeders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddHost();

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

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();

    app.UseSwagger();
    app.UseSwaggerUI(options =>
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
