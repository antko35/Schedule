namespace UserService.API.Extensions
{
    using Microsoft.EntityFrameworkCore;
    using UserService.Infrastructure;

    public static class MigrationExtensions
    {
        public static void ApplyMigrations(this IApplicationBuilder app)
        {
            using IServiceScope scope = app.ApplicationServices.CreateScope();
            using UserDbContext context = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        }
    }
}
