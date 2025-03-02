using Hangfire;
using Hangfire.Dashboard;
using MediatR;
using ScheduleService.Application.UseCases.Commands.Schedule;
using ScheduleService.Application.UseCases.Commands.ScheduleRules;

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

        recurringJobManager.AddOrUpdate(
            "create-schedule-rules",
            () => mediator.Send(new AddingScheduleRulesForNextMonthCommand(), CancellationToken.None),
            Cron.Monthly(1, 0));
    }
}