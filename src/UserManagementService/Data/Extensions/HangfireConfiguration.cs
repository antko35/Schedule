namespace UserManagementService.DataAccess.Extensions
{
    using Hangfire;
    using Hangfire.Mongo;
    using Hangfire.Mongo.Migration.Strategies.Backup;
    using Hangfire.Mongo.Migration.Strategies;
    using Microsoft.Extensions.DependencyInjection;
    using MongoDB.Driver;

    public static class HangfireConfiguration
    {
        public static IServiceCollection ConfigureHangfire(this IServiceCollection services)
        {
            services.AddHangfire((serviceProvider, config) =>
            {
                var mongoClient = serviceProvider.GetRequiredService<IMongoClient>();
                config.UseMongoStorage(
                    mongoClient, "HangfireDb", new MongoStorageOptions
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
}

