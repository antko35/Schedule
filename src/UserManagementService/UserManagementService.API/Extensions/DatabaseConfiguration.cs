using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using UserManagementService.Data.Database;

namespace UserManagementService.API.Extensions
{
    public static class DatabaseConfiguration
    {
        public static void ConfigureDatabase(
            this WebApplicationBuilder builder)
        {
            builder.Services.Configure<DbSettings>(builder.Configuration.GetSection("DatabaseSettings"));
            builder.Services.AddSingleton<IDbSettings>(sp =>
                sp.GetRequiredService<IOptions<DbSettings>>().Value);

            var dbSettings = builder.Configuration.GetSection("DatabaseSettings").Get<DbSettings>();
            builder.Services.AddSingleton(dbSettings);

            builder.Services.AddSingleton<DbContext>();
        }
    }
}
