namespace ScheduleService.PostgreSqlIntegration;

using Microsoft.EntityFrameworkCore;
using ScheduleService.DataAccess.Database;
using ScheduleService.DataAccess.Repository;
using ScheduleService.Domain.Models;
using Testcontainers.PostgreSql;
using Xunit;

public sealed class TrackedShiftReplacementTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer database = new PostgreSqlBuilder().WithImage("postgres:16").Build();

    public async Task InitializeAsync()
    {
        await database.StartAsync();
        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    public async Task DisposeAsync() => await database.DisposeAsync();

    [Fact]
    public async Task ReplacingTrackedWeekdayAndAddingSaturdayPersistsBothShifts()
    {
        var now = new DateTime(2026, 10, 1, 8, 0, 0, DateTimeKind.Utc);
        var period = new DateOnly(2026, 10, 1);
        var weekday = new DateOnly(2026, 10, 2);
        var saturday = new DateOnly(2026, 10, 3);
        var existingId = Guid.NewGuid();
        var schedule = new MonthlySchedule("schedule-saturday", "user-saturday", "department-saturday", period, now);
        schedule.SetShift(
            new WorkShift(
                existingId,
                schedule.Id,
                weekday,
                new DateTime(2026, 10, 2, 8, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 10, 2, 16, 0, 0, DateTimeKind.Utc)),
            now);

        await using (var context = CreateContext())
        {
            await new ScheduleRepository(context).AddAsync(schedule, CancellationToken.None);
            await context.SaveChangesAsync();
        }

        await using (var context = CreateContext())
        {
            var loaded = await new ScheduleRepository(context).GetByIdAsync(schedule.Id, CancellationToken.None);
            Assert.NotNull(loaded);
            loaded.SetShift(
                new WorkShift(
                    existingId,
                    schedule.Id,
                    weekday,
                    new DateTime(2026, 10, 2, 8, 0, 0, DateTimeKind.Utc),
                    new DateTime(2026, 10, 2, 14, 0, 0, DateTimeKind.Utc)),
                now.AddDays(1));
            loaded.SetShift(
                new WorkShift(
                    Guid.NewGuid(),
                    schedule.Id,
                    saturday,
                    new DateTime(2026, 10, 3, 9, 0, 0, DateTimeKind.Utc),
                    new DateTime(2026, 10, 3, 11, 0, 0, DateTimeKind.Utc)),
                now.AddDays(1));
            await context.SaveChangesAsync();
        }

        await using (var context = CreateContext())
        {
            var loaded = await new ScheduleRepository(context).GetByIdAsync(schedule.Id, CancellationToken.None);
            Assert.NotNull(loaded);
            Assert.Equal(2, loaded.WorkShifts.Count);
            var savedWeekday = Assert.Single(loaded.WorkShifts, shift => shift.WorkDate == weekday);
            Assert.Equal(existingId, savedWeekday.Id);
            Assert.Equal(new DateTime(2026, 10, 2, 14, 0, 0, DateTimeKind.Utc), savedWeekday.EndsAtUtc);
            Assert.Single(loaded.WorkShifts, shift => shift.WorkDate == saturday);
        }
    }

    private ScheduleDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ScheduleDbContext>()
            .UseNpgsql(
                database.GetConnectionString(),
                provider => provider.MigrationsHistoryTable("__EFMigrationsHistory", ScheduleDbContext.SchemaName))
            .Options;
        return new ScheduleDbContext(options);
    }
}
