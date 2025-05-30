using Hangfire;
using MediatR;
using System.Reflection;
using Infrastructure.Extensions;
using UserManagementService.API.Extensions;
using UserManagementService.API.Extensions.AppExtensions;
using UserManagementService.API.Extensions.AppExtensions.Hangfire;
using UserManagementService.Application.Extensions;
using UserManagementService.Application.UseCases.Commands.User;
using UserManagementService.Data.Extensions;
using UserManagementService.DataAccess.Database;
using UserManagementService.DataAccess.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// API Layer
builder.ConfigureGrpc();

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
builder.Services.AddSwaggerGen(options =>
{
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

var app = builder.Build();

app.UseMiddleware<ExeptionHadlingMiddleware>();

app.ConfigureSwagger();

app.ConfigureHangfire();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
