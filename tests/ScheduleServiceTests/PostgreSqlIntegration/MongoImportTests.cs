namespace ScheduleService.PostgreSqlIntegration;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using ScheduleMongoImport;
using ScheduleService.DataAccess.Database;
using Testcontainers.MongoDb;
using Testcontainers.PostgreSql;
using Xunit;

public sealed class MongoImportTests
{
    [Fact]
    public async Task RealLegacyBsonDryRunApplyRepeatAndConflictAreSafe()
    {
        await using var postgres = new PostgreSqlBuilder().WithImage("postgres:17.6-alpine").Build();
        await using var mongo = new MongoDbBuilder().WithImage("mongo:7").Build();
        await postgres.StartAsync();
        await mongo.StartAsync();
        var options = new DbContextOptionsBuilder<ScheduleDbContext>().UseNpgsql(postgres.GetConnectionString(),
            x => x.MigrationsHistoryTable("__EFMigrationsHistory", "schedule")).Options;
        await using var db = new ScheduleDbContext(options);
        await db.Database.MigrateAsync();
        var source = new MongoClient(mongo.GetConnectionString()).GetDatabase("legacy");
        var scheduleId = ObjectId.GenerateNewId();
        var ruleId = ObjectId.GenerateNewId();
        var shift = new LegacyWorkDay { Day = 5, StartTime = new DateTimeOffset(2026, 10, 5, 8, 0, 0, TimeSpan.Zero), EndTime = new DateTimeOffset(2026, 10, 5, 16, 0, 0, TimeSpan.Zero) };
        await source.GetCollection<BsonDocument>("Schedule").InsertOneAsync(new BsonDocument
        {
            { "_id", scheduleId }, { "monthName", "october" }, { "workDays", new BsonArray { shift.ToBsonDocument() } },
        });
        await source.GetCollection<BsonDocument>("UserScheduleRule").InsertOneAsync(new BsonDocument
        {
            { "_id", ruleId }, { "userId", "u" }, { "departmentId", "d" }, { "monthName", "october" }, { "year", 2026 },
            { "hoursPerMonth", 160.0 }, { "hoursPerDay", 8.0 }, { "startWorkDayTime", BsonSerializer.Deserialize<BsonDocument>(new LegacyTime { Value = new TimeOnly(8, 0) }.ToBson())["Value"] },
            { "onlyFirstShift", true }, { "scheduleId", scheduleId },
        });
        var dates = new LegacyDates { Holiday = new DateOnly(2026, 1, 1), Transfer = DateOnly.MinValue }.ToBsonDocument();
        await source.GetCollection<BsonDocument>("Calendar").InsertOneAsync(new BsonDocument
        {
            { "_id", ObjectId.GenerateNewId() }, { "holiday", dates["Holiday"] }, { "transferDay", dates["Transfer"] },
        });
        var environment = new Dictionary<string, string>
        {
            ["SCHEDULE_IMPORT_MONGO_URI"] = mongo.GetConnectionString(), ["SCHEDULE_IMPORT_MONGO_DATABASE"] = "legacy",
            ["SCHEDULE_IMPORT_POSTGRES"] = postgres.GetConnectionString(), ["SCHEDULE_IMPORT_TIME_ZONE"] = "Europe/Warsaw",
        };
        Assert.Equal(0, await ImportRunner.RunAsync([], environment));
        Assert.Equal(0, await db.MonthlySchedules.CountAsync());
        Assert.Equal(0, await ImportRunner.RunAsync(["--apply"], environment));
        Assert.Equal(0, await ImportRunner.RunAsync(["--apply"], environment));
        Assert.Equal(1, await db.MonthlySchedules.CountAsync());
        Assert.Equal(scheduleId.ToString(), (await db.MonthlySchedules.SingleAsync()).Id);
        Assert.Equal(1, await db.WorkShifts.CountAsync());
        Assert.Null((await db.CalendarExceptions.SingleAsync()).TransferDate);
        await source.GetCollection<BsonDocument>("UserScheduleRule").UpdateOneAsync(
            new BsonDocument("_id", ruleId), Builders<BsonDocument>.Update.Set("hoursPerMonth", 168.0));
        Assert.Equal(1, await ImportRunner.RunAsync(["--apply"], environment));
        Assert.Equal(160 * 60, (await db.ScheduleGenerationRules.SingleAsync()).TargetMinutes);
        Assert.Equal(1, await source.GetCollection<BsonDocument>("Schedule").CountDocumentsAsync(FilterDefinition<BsonDocument>.Empty));
    }

    public sealed class LegacyWorkDay
    {
        public int Day { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
    }
    public sealed class LegacyTime { public TimeOnly Value { get; set; } }
    public sealed class LegacyDates { public DateOnly Holiday { get; set; } public DateOnly? Transfer { get; set; } }
}
