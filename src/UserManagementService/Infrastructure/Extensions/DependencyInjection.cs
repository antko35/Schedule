using Microsoft.Extensions.DependencyInjection;
using UserManagementService.Domain.Abstractions.IRabbitMq;

namespace Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureDependencies(this IServiceCollection services)
    {
        services.AddScoped<IUserEventPublisher, UserEventPublisher>();
            
        return services;
    }
}