namespace ScheduleService.Application.Extensions
{
    using System.Reflection;
    using FluentValidation;
    using MediatR;
    using Microsoft.Extensions.DependencyInjection;
    using ScheduleService.Application.Extensions.Validation;

    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicatiobLayerDependencis(this IServiceCollection services)
        {
            services.AddMediatR(cnf => cnf.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            return services;
        }
    }
}
