namespace ScheduleService.PostgreSqlIntegration;

using Microsoft.EntityFrameworkCore;
using ScheduleService.DataAccess.Database;
using ScheduleService.DataAccess.Repository;
using ScheduleService.Domain.Models;
using Testcontainers.PostgreSql;
using Xunit;

public sealed class SchedulePersistenceTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer database = new PostgreSqlBuilder().WithImage("postgres:16").Build();

    public async Task InitializeAsync()
    {
        await database.StartAsync();
        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await database.DisposeAsync();
    }

    [Fact]
    public async Task MigrationCreatesSchemaAndRepositoryRoundTripsScheduleWithShift()
    {
        var now = new DateTime(2026, 10, 1, 8, 0, 0, DateTimeKind.Utc);
        var schedule = new MonthlySchedule("schedule-1", "user-1", "department-1", new DateOnly(2026, 10, 1), now);
        schedule.SetShift(
            new WorkShift(
                Guid.NewGuid(),
                schedule.Id,
                new DateOnly(2026, 10, 3),
                new DateTime(2026, 10, 3, 8, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 10, 3, 16, 0, 0, DateTimeKind.Utc)),
            now);

        await using (var context = CreateContext())
        {
            Assert.Contains("20261024070902_InitialScheduleSchema", await context.Database.GetAppliedMigrationsAsync());
            await new ScheduleRepository(context).AddAsync(schedule, CancellationToken.None);
            await context.SaveChangesAsync();
        }

        await using (var context = CreateContext())
        {
            var loaded = await new ScheduleRepository(context).GetAsync(
                "user-1", "department-1", new DateOnly(2026, 10, 1), CancellationToken.None);

            Assert.NotNull(loaded);
            Assert.Single(loaded.WorkShifts);
            Assert.Equal(new DateOnly(2026, 10, 3), loaded.WorkShifts.Single().WorkDate);
            Assert.Equal(DateTimeKind.Utc, loaded.WorkShifts.Single().StartsAtUtc.Kind);
        }
    }

    [Fact]
    public async Task DatabaseRejectsDuplicateScheduleForUserDepartmentAndPeriod()
    {
        var now = new DateTime(2026, 10, 1, 8, 0, 0, DateTimeKind.Utc);
        var period = new DateOnly(2026, 11, 1);

        await using (var context = CreateContext())
        {
            context.MonthlySchedules.Add(new MonthlySchedule("first", "user-2", "department-2", period, now));
            await context.SaveChangesAsync();
        }

        await using (var context = CreateContext())
        {
            context.MonthlySchedules.Add(new MonthlySchedule("second", "user-2", "department-2", period, now));
            await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
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
