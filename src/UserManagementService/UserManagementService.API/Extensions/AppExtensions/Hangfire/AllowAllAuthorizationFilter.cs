using Hangfire.Dashboard;

public class AllowAllAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        // Разрешить доступ всем
        return true;
    }
}