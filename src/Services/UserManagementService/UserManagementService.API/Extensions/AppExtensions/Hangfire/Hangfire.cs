using Hangfire;
using UserManagementService.Application.Jobs;
using UserManagementService.Application.UseCases.Commands.User;

namespace UserManagementService.API.Extensions.AppExtensions.Hangfire
{
    public static class Hangfire
    {
        public static void ConfigureHangfire(this WebApplication app)
        {
            // Настраиваем Dashboard
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] { new AllowAllAuthorizationFilter() }
            });

            // Создаем задачу в области видимости
            using var scope = app.Services.CreateScope();
            var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

            // Создаем повторяющуюся задачу
            recurringJobManager.AddOrUpdate<UpdateUserAgesJob>(
                "update-ages-daily",
                job => job.ExecuteAsync(CancellationToken.None),
                Cron.Daily);
        }
    }
}
