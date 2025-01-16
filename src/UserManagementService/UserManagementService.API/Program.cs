using System.Reflection;
using UserManagementService.API.Extensions;
using UserManagementService.Application.Extensions;
using UserManagementService.Data.Extensions;
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
    .AddDataLayerDependencis();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

var app = builder.Build();

app.UseMiddleware<ExeptionHadlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => // UseSwaggerUI is called only in Development.
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
