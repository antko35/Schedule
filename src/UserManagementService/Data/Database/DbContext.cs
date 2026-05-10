namespace UserManagementService.DataAccess.Database;

using MongoDB.Driver;

public class DbContext
{
    private readonly IMongoDatabase? database;

    public DbContext(IMongoClient mongoClient, BaseDbOptions settings)
    {
        database = mongoClient.GetDatabase(settings.DatabaseName);
    }

    public IMongoDatabase? Database => database;
}
