namespace UserManagementService.Application.Extensions;

using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
public static class DependencyInjection
{
    public static IServiceCollection AddApplicatiobLayerDependencis(this IServiceCollection services)
    {
        services.AddMediatR(cnf => cnf.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        return services;
    }
}
