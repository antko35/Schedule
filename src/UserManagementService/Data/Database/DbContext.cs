namespace UserManagementService.Data.Database;

using Microsoft.Extensions.Options;
using MongoDB.Driver;

public class DbContext
{
    private readonly IMongoDatabase? _database;

    public DbContext(DbSettings settings)
    {
        var mongoURI = MongoUrl.Create(settings.ConnectionString);
        var mongoClient = new MongoClient(mongoURI);
        _database = mongoClient.GetDatabase(settings.DatabaseName);
    }

    public IMongoDatabase? Database => _database;
}
