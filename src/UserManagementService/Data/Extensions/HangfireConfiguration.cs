namespace UserManagementService.DataAccess.Extensions
{
    using Hangfire;
    using Hangfire.Mongo;
    using Hangfire.Mongo.Migration.Strategies.Backup;
    using Hangfire.Mongo.Migration.Strategies;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading.Tasks;
    using UserManagementService.DataAccess.Database;
    using Microsoft.Extensions.Options;

    public static class HangfireConfiguration
    {
        public static IServiceCollection ConfigureHangfire(this IServiceCollection services)
        {
            services.AddHangfire((serviceProvider, config) =>
            {
                var dbOptions = serviceProvider.GetService<IOptions<DbOptions>>()?.Value;
                config.UseMongoStorage(dbOptions.ConnectionString, "HangfireDb", new MongoStorageOptions
                {
                    MigrationOptions = new MongoMigrationOptions
                    {
                        MigrationStrategy = new DropMongoMigrationStrategy(),
                        BackupStrategy = new CollectionMongoBackupStrategy()
                    },
                    Prefix = "hangfire", // Префикс для коллекций
                    CheckQueuedJobsStrategy = CheckQueuedJobsStrategy.TailNotificationsCollection,
                });
            });

            services.AddHangfireServer();

            return services;
        }
    }
}
