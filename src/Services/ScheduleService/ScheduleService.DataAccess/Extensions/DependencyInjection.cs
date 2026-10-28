namespace ScheduleService.DataAccess.Extensions;

using Microsoft.Extensions.DependencyInjection;
using ScheduleService.DataAccess.Repository;
using ScheduleService.Domain.Abstractions;

public static class DependencyInjection
{
    public static IServiceCollection AddDataAccessDependencis(this IServiceCollection services)
    {
        services.AddScoped<ICalendarRepository, CalendarRepository>();
        services.AddScoped<IScheduleGenerationRuleRepository, ScheduleGenerationRuleRepository>();
        services.AddScoped<IScheduleRepository, ScheduleRepository>();

        return services;
    }
}
