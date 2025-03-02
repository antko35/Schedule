using Hangfire.Dashboard;

namespace ScheduleService.API.Extensions.AppExtensions.Hangfire;

public class AllowAllAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        // Разрешить доступ всем
        return true;
    }
}