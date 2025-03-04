using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ScheduleService.DataAccess.Database;

namespace ScheduleService.DataAccess.Extensions;

public static class HangfireConfiguration
{
    public static IServiceCollection HangfireConfigure(this IServiceCollection services)
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
                Prefix = "hangfire",
                CheckQueuedJobsStrategy = CheckQueuedJobsStrategy.TailNotificationsCollection,
            });
        });

        services.AddHangfireServer();

        return services;
    }
}