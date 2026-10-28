using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Bson.Serialization;
using ScheduleService.DataAccess.Database;
using ScheduleService.Domain.Models;

namespace ScheduleMongoImport;

public static class ImportRunner
{
    public static async Task<int> RunAsync(string[] args, IReadOnlyDictionary<string, string>? environment = null)
    {
        string GetEnvironment(string name) => environment is not null
            ? environment.TryGetValue(name, out var value) && !string.IsNullOrWhiteSpace(value)
                ? value : throw new ArgumentException($"Set {name} before running the import.")
            : RequiredEnvironment(name);
        try
        {
            var apply = args.Contains("--apply", StringComparer.Ordinal);
            if (args.Any(arg => arg != "--apply"))
            {
                throw new ArgumentException("Only --apply is supported. Omit it for a dry run.");
            }

            var mongoUri = GetEnvironment("SCHEDULE_IMPORT_MONGO_URI");
            var mongoDatabase = GetEnvironment("SCHEDULE_IMPORT_MONGO_DATABASE");
            var postgres = GetEnvironment("SCHEDULE_IMPORT_POSTGRES");
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(GetEnvironment("SCHEDULE_IMPORT_TIME_ZONE"));

            var source = new MongoClient(mongoUri).GetDatabase(mongoDatabase);
            var ruleDocuments = await source.GetCollection<BsonDocument>("UserScheduleRule")
                .Find(FilterDefinition<BsonDocument>.Empty).ToListAsync();
            var scheduleDocuments = await source.GetCollection<BsonDocument>("Schedule")
                .Find(FilterDefinition<BsonDocument>.Empty).ToListAsync();
            var calendarDocuments = await source.GetCollection<BsonDocument>("Calendar")
                .Find(FilterDefinition<BsonDocument>.Empty).ToListAsync();

            if (ruleDocuments.Count == 0 && scheduleDocuments.Count == 0 && calendarDocuments.Count == 0)
            {
                throw new InvalidOperationException(
                    "All source collections are empty. Check the Mongo URI and database before importing.");
            }

            var mapped = MapAll(ruleDocuments, scheduleDocuments, calendarDocuments, timeZone);

            var options = new DbContextOptionsBuilder<ScheduleDbContext>()
                .UseNpgsql(postgres, provider => provider.MigrationsHistoryTable(
                    "__EFMigrationsHistory", ScheduleDbContext.SchemaName))
                .Options;
            await using var db = new ScheduleDbContext(options);
            var pending = await db.Database.GetPendingMigrationsAsync();
            if (pending.Any())
            {
                throw new InvalidOperationException("Apply the Schedule EF migration before importing data.");
            }

            await CheckDestinationAsync(db, mapped);
            Console.WriteLine($"Validated source: {mapped.Rules.Count} rules, {mapped.Schedules.Count} schedules, " +
                $"{mapped.Schedules.Sum(schedule => schedule.WorkShifts.Count)} shifts, {mapped.Calendar.Count} calendar exceptions.");

            if (!apply)
            {
                Console.WriteLine("Dry run complete. No records were written. Pass --apply to import.");
                return 0;
            }

            await using var transaction = await db.Database.BeginTransactionAsync();
            foreach (var rule in mapped.Rules)
            {
                if (!await db.ScheduleGenerationRules.AnyAsync(item => item.Id == rule.Id))
                {
                    db.ScheduleGenerationRules.Add(rule);
                }
            }

            foreach (var schedule in mapped.Schedules)
            {
                if (!await db.MonthlySchedules.AnyAsync(item => item.Id == schedule.Id))
                {
                    db.MonthlySchedules.Add(schedule);
                }
            }

            foreach (var calendar in mapped.Calendar)
            {
                if (!await db.CalendarExceptions.AnyAsync(item => item.Id == calendar.Id))
                {
                    db.CalendarExceptions.Add(calendar);
                }
            }

            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            await CheckDestinationAsync(db, mapped, requireAll: true);
            Console.WriteLine("Import committed and all source records verified in PostgreSQL.");
            return 0;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine($"Import stopped: {error.Message}");
            return 1;
        }
    }

    private static string RequiredEnvironment(string name)
    {
        var value = Environment.GetEnvironmentVariable(name);
        return string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException($"Set {name} before running the import.")
            : value;
    }

    private static ImportSet MapAll(
        IReadOnlyList<BsonDocument> ruleDocuments,
        IReadOnlyList<BsonDocument> scheduleDocuments,
        IReadOnlyList<BsonDocument> calendarDocuments,
        TimeZoneInfo timeZone)
    {
        var schedulesById = scheduleDocuments.ToDictionary(Id, StringComparer.Ordinal);
        var referencedSchedules = new HashSet<string>(StringComparer.Ordinal);
        var rules = new List<ScheduleGenerationRule>();
        var schedules = new List<MonthlySchedule>();
        var now = DateTime.UtcNow;

        foreach (var source in ruleDocuments)
        {
            var id = Id(source);
            var period = ParsePeriod(RequiredString(source, "monthName"), RequiredInt(source, "year"));
            var userId = RequiredString(source, "userId");
            var departmentId = RequiredString(source, "departmentId");
            var pattern = ParsePattern(source);
            var rule = new ScheduleGenerationRule(
                id, userId, departmentId, period,
                HoursToMinutes(RequiredDouble(source, "hoursPerMonth"), id),
                HoursToMinutes(RequiredDouble(source, "hoursPerDay"), id),
                OptionalTime(source, "startWorkDayTime", new TimeOnly(8, 0)),
                new TimeOnly(14, 30), pattern);
            rules.Add(rule);

            var scheduleId = OptionalString(source, "scheduleId");
            if (string.IsNullOrEmpty(scheduleId))
            {
                continue;
            }

            if (!schedulesById.TryGetValue(scheduleId, out var scheduleSource))
            {
                throw new InvalidOperationException($"Rule {id} references missing schedule {scheduleId}.");
            }

            if (!referencedSchedules.Add(scheduleId))
            {
                throw new InvalidOperationException($"Schedule {scheduleId} is referenced by multiple rules.");
            }

            if (ParsePeriod(RequiredString(scheduleSource, "monthName"), period.Year) != period)
            {
                throw new InvalidOperationException($"Rule {id} and schedule {scheduleId} have different months.");
            }

            var schedule = new MonthlySchedule(scheduleId, userId, departmentId, period, now);
            var shifts = new List<WorkShift>();
            foreach (var day in OptionalArray(scheduleSource, "workDays"))
            {
                var document = day.AsBsonDocument;
                var workDate = new DateOnly(period.Year, period.Month, RequiredInt(document, "Day"));
                var startUtc = RequiredUtc(document, "StartTime");
                var endUtc = RequiredUtc(document, "EndTime");
                if (DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(startUtc, timeZone)) != workDate)
                {
                    throw new InvalidOperationException(
                        $"Schedule {scheduleId} shift {workDate} has a start date outside its local work date. Check time zone.");
                }

                shifts.Add(new WorkShift(ShiftId(scheduleId, workDate), scheduleId, workDate, startUtc, endUtc));
            }

            schedule.ReplaceShifts(shifts, now);
            schedules.Add(schedule);
        }

        var unreferenced = schedulesById.Keys.Except(referencedSchedules, StringComparer.Ordinal).FirstOrDefault();
        if (unreferenced is not null)
        {
            throw new InvalidOperationException($"Mongo schedule {unreferenced} has no owning rule.");
        }

        var calendar = calendarDocuments.Select(source =>
        {
            var transferDate = OptionalDate(source, "transferDay");
            if (transferDate == DateOnly.MinValue)
            {
                transferDate = null;
            }

            return new CalendarException(Id(source), RequiredDate(source, "holiday"), transferDate);
        }).ToList();

        EnsureUnique(rules, rule => $"{rule.UserId}/{rule.DepartmentId}/{rule.Period:yyyy-MM-dd}", "rules");
        EnsureUnique(schedules, schedule => $"{schedule.UserId}/{schedule.DepartmentId}/{schedule.Period:yyyy-MM-dd}", "schedules");
        EnsureUnique(calendar, item => item.HolidayDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), "calendar dates");
        return new ImportSet(rules, schedules, calendar);
    }

    private static async Task CheckDestinationAsync(ScheduleDbContext db, ImportSet mapped, bool requireAll = false)
    {
        foreach (var source in mapped.Rules)
        {
            var existing = await db.ScheduleGenerationRules.AsNoTracking().SingleOrDefaultAsync(item => item.Id == source.Id);
            if (existing is null)
            {
                if (requireAll)
                {
                    throw new InvalidOperationException($"Destination rule {source.Id} is missing.");
                }

                if (await db.ScheduleGenerationRules.AsNoTracking().AnyAsync(item =>
                    item.UserId == source.UserId && item.DepartmentId == source.DepartmentId && item.Period == source.Period))
                {
                    throw new InvalidOperationException($"Destination already has rule for {source.UserId}/{source.DepartmentId}/{source.Period} with another ID.");
                }

                continue;
            }

            if (existing.UserId != source.UserId ||
                existing.DepartmentId != source.DepartmentId || existing.Period != source.Period ||
                existing.TargetMinutes != source.TargetMinutes || existing.MaxShiftMinutes != source.MaxShiftMinutes ||
                existing.FirstShiftStart != source.FirstShiftStart || existing.SecondShiftStart != source.SecondShiftStart ||
                existing.Pattern != source.Pattern)
            {
                throw new InvalidOperationException($"Destination rule {source.Id} differs from Mongo.");
            }
        }

        foreach (var source in mapped.Schedules)
        {
            var existing = await db.MonthlySchedules.AsNoTracking()
                .Include(item => item.WorkShifts).SingleOrDefaultAsync(item => item.Id == source.Id);
            if (existing is null)
            {
                if (requireAll)
                {
                    throw new InvalidOperationException($"Destination schedule {source.Id} is missing.");
                }

                if (await db.MonthlySchedules.AsNoTracking().AnyAsync(item =>
                    item.UserId == source.UserId && item.DepartmentId == source.DepartmentId && item.Period == source.Period))
                {
                    throw new InvalidOperationException($"Destination already has schedule for {source.UserId}/{source.DepartmentId}/{source.Period} with another ID.");
                }

                continue;
            }

            if (existing.UserId != source.UserId || existing.DepartmentId != source.DepartmentId ||
                existing.Period != source.Period || existing.WorkShifts.Count != source.WorkShifts.Count ||
                source.WorkShifts.Any(shift => !existing.WorkShifts.Any(other =>
                    other.Id == shift.Id && other.WorkDate == shift.WorkDate &&
                    other.StartsAtUtc == shift.StartsAtUtc && other.EndsAtUtc == shift.EndsAtUtc)))
            {
                throw new InvalidOperationException($"Destination schedule {source.Id} differs from Mongo.");
            }
        }

        foreach (var source in mapped.Calendar)
        {
            var existing = await db.CalendarExceptions.AsNoTracking().SingleOrDefaultAsync(item => item.Id == source.Id);
            if (existing is null)
            {
                if (requireAll)
                {
                    throw new InvalidOperationException($"Destination calendar exception {source.Id} is missing.");
                }

                if (await db.CalendarExceptions.AsNoTracking().AnyAsync(item => item.HolidayDate == source.HolidayDate))
                {
                    throw new InvalidOperationException($"Destination already has calendar date {source.HolidayDate} with another ID.");
                }

                continue;
            }

            if (existing.HolidayDate != source.HolidayDate || existing.TransferDate != source.TransferDate)
            {
                throw new InvalidOperationException($"Destination calendar exception {source.Id} differs from Mongo.");
            }
        }
    }

    private static void EnsureUnique<T>(IEnumerable<T> values, Func<T, string> key, string label)
    {
        var duplicate = values.GroupBy(key, StringComparer.Ordinal).FirstOrDefault(group => group.Count() > 1);
        if (duplicate is not null)
        {
            throw new InvalidOperationException($"Duplicate {label} key: {duplicate.Key}.");
        }
    }

    private static Guid ShiftId(string scheduleId, DateOnly date)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes($"{scheduleId}/{date:yyyy-MM-dd}"));
        return new Guid(bytes.AsSpan(0, 16));
    }

    private static DateOnly ParsePeriod(string month, int year)
    {
        var monthNumber = DateTime.ParseExact(month, "MMMM", CultureInfo.InvariantCulture).Month;
        return new DateOnly(year, monthNumber, 1);
    }

    private static int HoursToMinutes(double hours, string id)
    {
        var minutes = hours * 60;
        if (minutes <= 0 || minutes > int.MaxValue || Math.Abs(minutes - Math.Round(minutes)) > 0.001)
        {
            throw new InvalidOperationException($"Rule {id} has hours that cannot be represented as positive whole minutes.");
        }

        return checked((int)Math.Round(minutes));
    }

    private static ShiftPattern ParsePattern(BsonDocument source)
    {
        var flags = new[]
        {
            ("onlyFirstShift", ShiftPattern.FirstShiftOnly),
            ("onlySecondShift", ShiftPattern.SecondShiftOnly),
            ("EvenDOW", ShiftPattern.AlternateByWeekdayA),
            ("UnEvenDOW", ShiftPattern.AlternateByWeekdayB),
            ("EvenDOM", ShiftPattern.AlternateByMonthDayA),
            ("UnEvenDOM", ShiftPattern.AlternateByMonthDayB),
        };
        var active = flags.Where(flag => source.TryGetValue(flag.Item1, out var value) && value.ToBoolean()).ToList();
        if (active.Count > 1)
        {
            throw new InvalidOperationException($"Rule {Id(source)} has conflicting shift flags.");
        }

        return active.Count == 0 ? ShiftPattern.Manual : active[0].Item2;
    }

    private static string Id(BsonDocument source) => source["_id"].IsObjectId
        ? source["_id"].AsObjectId.ToString()
        : source["_id"].AsString;

    private static string RequiredString(BsonDocument source, string name) =>
        !source.TryGetValue(name, out var value) || value.IsBsonNull || string.IsNullOrWhiteSpace(value.AsString)
            ? throw new InvalidOperationException($"Document {Id(source)} has no {name}.")
            : value.AsString;

    private static string? OptionalString(BsonDocument source, string name) =>
        !source.TryGetValue(name, out var value) || value.IsBsonNull
            ? null
            : value.IsObjectId ? value.AsObjectId.ToString() : value.AsString;

    private static int RequiredInt(BsonDocument source, string name) =>
        !source.TryGetValue(name, out var value) || value.IsBsonNull
            ? throw new InvalidOperationException($"Document {Id(source)} has no {name}.")
            : value.ToInt32();

    private static double RequiredDouble(BsonDocument source, string name) =>
        !source.TryGetValue(name, out var value) || value.IsBsonNull
            ? throw new InvalidOperationException($"Document {Id(source)} has no {name}.")
            : value.ToDouble();

    private static IEnumerable<BsonValue> OptionalArray(BsonDocument source, string name) =>
        !source.TryGetValue(name, out var value) || value.IsBsonNull
            ? [] : value.AsBsonArray;

    private static DateTime RequiredUtc(BsonDocument source, string name)
    {
        if (!source.TryGetValue(name, out var value) || value.IsBsonNull)
            throw new InvalidOperationException($"Shift has no BSON date-time {name}.");
        if (value.IsBsonDateTime)
            return value.AsBsonDateTime.ToUniversalTime();
        // MongoDB's legacy DateTimeOffset serializer stores ticks and offset, not BSON DateTime.
        if (value.IsBsonArray || value.IsBsonDocument || value.IsString)
            return BsonSerializer.Deserialize<SerializedValue<DateTimeOffset>>(
                new BsonDocument("Value", value)).Value.UtcDateTime;
        throw new InvalidOperationException($"Shift has unsupported BSON date-time {name}.");
    }

    private static DateOnly RequiredDate(BsonDocument source, string name) =>
        OptionalDate(source, name) ?? throw new InvalidOperationException($"Document {Id(source)} has no {name}.");

    private static DateOnly? OptionalDate(BsonDocument source, string name)
    {
        if (!source.TryGetValue(name, out var value) || value.IsBsonNull)
        {
            return null;
        }

        if (value.IsBsonDateTime)
        {
            return DateOnly.FromDateTime(value.AsBsonDateTime.ToUniversalTime());
        }

        if (value.IsString)
        {
            return DateOnly.Parse(value.AsString, CultureInfo.InvariantCulture);
        }

        if (value.IsBsonDocument)
        {
            var document = value.AsBsonDocument;
            return new DateOnly(document["Year"].ToInt32(), document["Month"].ToInt32(), document["Day"].ToInt32());
        }

        throw new InvalidOperationException($"Document {Id(source)} has unsupported {name} representation.");
    }

    private static TimeOnly OptionalTime(BsonDocument source, string name, TimeOnly fallback)
    {
        if (!source.TryGetValue(name, out var value) || value.IsBsonNull)
        {
            return fallback;
        }

        if (value.IsString)
        {
            return TimeOnly.Parse(value.AsString, CultureInfo.InvariantCulture);
        }

        if (value.IsInt64)
        {
            return new TimeOnly(value.AsInt64);
        }

        if (value.IsBsonDocument)
        {
            var document = value.AsBsonDocument;
            if (document.TryGetValue("Ticks", out var ticks))
            {
                return new TimeOnly(ticks.ToInt64());
            }

            return new TimeOnly(document["Hour"].ToInt32(), document["Minute"].ToInt32(),
                document.GetValue("Second", 0).ToInt32());
        }

        throw new InvalidOperationException($"Rule {Id(source)} has unsupported {name} representation.");
    }

    public sealed class SerializedValue<T>
    {
        public T Value { get; set; } = default!;
    }

    private sealed record ImportSet(
        List<ScheduleGenerationRule> Rules,
        List<MonthlySchedule> Schedules,
        List<CalendarException> Calendar);
}

internal static class Program
{
    private static Task<int> Main(string[] args) => ImportRunner.RunAsync(args);
}
