using System.Reflection;
using ScheduleService.API.Extensions;
using ScheduleService.API.Extensions.AppExtensions.Hangfire;
using ScheduleService.Application.Extensions;
using ScheduleService.DataAccess.Extensions;
using ScheduleService.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplicatiobLayerDependencis();

builder.Services
    .ConfigureDb(builder.Configuration)
    .HangfireConfigure()
    .AddDataAccessDependencis();

builder.Services
    .AddInfrastructureDependencis(builder.Configuration);

builder.Services
    .AddSwaggerExtension();

var app = builder.Build();

app.UseMiddleware<ExeptionHadlingMiddleware>();

app.ConfigureHangfire();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
