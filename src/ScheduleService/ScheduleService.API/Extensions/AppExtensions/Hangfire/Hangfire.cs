using Hangfire;
using Hangfire.Dashboard;
using MediatR;
using ScheduleService.Application.UseCases.Commands.Schedule;

namespace ScheduleService.API.Extensions.AppExtensions.Hangfire;

public static class Hangfire
{
    public static void ConfigureHangfire(this WebApplication app)
    {
        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            Authorization = new[] { new AllowAllAuthorizationFilter() },
        });

        using var scope = app.Services.CreateScope();
        var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        recurringJobManager.AddOrUpdate(
            "send-notification-about-schedule-generation",
            () => mediator.Send(new SendPromptMonthlyCommand(), CancellationToken.None),
            Cron.Monthly(27, 15));
    }
}