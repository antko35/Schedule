using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ScheduleService.Application.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicatiobLayerDependencis(this IServiceCollection services)
        {
            services.AddMediatR(cnf => cnf.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

            return services;
        }
    }
}
