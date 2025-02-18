
namespace UserManagementService.Data.Extensions
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using UserManagementService.DataAccess.Database;
    using UserManagementService.DataAccess.Repository;
    using UserManagementService.Domain.Abstractions.IRepository;

    public static class DependencyInjection
    {
        public static IServiceCollection AddDataLayerDependencis(this IServiceCollection services)
        {
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IDepartmentRepository, DepartmentRepository>();
            services.AddScoped<IUserJobsRepository, UserJobsRepository>();
            services.AddScoped<IClinicRepository, ClinicRepository>();

            return services;
        }
    }
}
