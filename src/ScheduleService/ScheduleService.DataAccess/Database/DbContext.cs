using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScheduleService.DataAccess.Database
{
    public class DbContext
    {
        private readonly IMongoDatabase? database;

        public DbContext(DbOptions settings)
        {
            var mongoURI = MongoUrl.Create(settings.ConnectionString);
            var mongoClient = new MongoClient(mongoURI);
            database = mongoClient.GetDatabase(settings.DatabaseName);
        }

        public IMongoDatabase? Database => database;
    }
}
