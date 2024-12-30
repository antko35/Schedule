using Microsoft.AspNetCore.Identity;
using UserService.API.Extensions;
using UserService.Domain.Models;
using UserService.Infrastructure;
using UserService.Infrastructure.Extensions;
using UserService.Infrastructure.Seeders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
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
    app.UseSwagger();
    app.UseSwaggerUI(options => // UseSwaggerUI is called only in Development.
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = string.Empty;
    });

    app.ApplyMigrations();
}

app.UseHttpsRedirection();

app.UseCors();

app.MapGroup("userService").MapIdentityApi<User>();

app.UseAuthorization();

app.MapControllers();

app.Run();
