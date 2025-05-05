using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ScheduleService.DataAccess.EmailSender;
using ScheduleService.Domain.Abstractions.Rabbit;
using ScheduleService.Infrastructure.EmailSender;

namespace ScheduleService.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureDependencis(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IEmailService, SmtpEmailService>();
        services.Configure<SmtpSettings>(configuration.GetSection("SmtpSettings"));

        services.AddScoped<IUserEmailsRpcClient, UserEmailsRpcClient>();
        
        services.AddHostedService<UserCreatedConsumer>();
        services.AddHostedService<UserDeletedConsumer>();
        
        return services;
    }
}