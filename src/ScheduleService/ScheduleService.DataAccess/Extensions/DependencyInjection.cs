using Microsoft.Extensions.DependencyInjection;
using ScheduleService.DataAccess.Repository;
using ScheduleService.Domain.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScheduleService.DataAccess.EmailSender;

namespace ScheduleService.DataAccess.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDataAccessDependencis(this IServiceCollection services)
        {
            services.AddScoped<ICalendarRepository, CalendarRepository>();
            services.AddScoped<IUserRuleRepository, UserRuleRepository>();
            services.AddScoped<IScheduleRepository, ScheduleRepository>();

            return services;
        }
    }
}
