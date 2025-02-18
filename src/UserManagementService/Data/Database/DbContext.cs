namespace UserManagementService.DataAccess.Database;

using Microsoft.Extensions.Options;
using MongoDB.Driver;

public class DbContext
{
    private readonly IMongoDatabase? database;

    public DbContext(BaseDbOptions settings)
    {
        var mongoURI = MongoUrl.Create(settings.ConnectionString);
        var mongoClient = new MongoClient(mongoURI);
        database = mongoClient.GetDatabase(settings.DatabaseName);
    }

    public IMongoDatabase? Database => database;
}
