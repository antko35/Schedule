namespace ScheduleService.DataAccess.Database;

using Microsoft.EntityFrameworkCore;
using ScheduleService.Domain.Models;

public class ScheduleDbContext : DbContext
{
    public const string SchemaName = "schedule";

    public ScheduleDbContext(DbContextOptions<ScheduleDbContext> options)
        : base(options)
    {
    }

    public DbSet<MonthlySchedule> MonthlySchedules => Set<MonthlySchedule>();

    public DbSet<WorkShift> WorkShifts => Set<WorkShift>();

    public DbSet<ScheduleGenerationRule> ScheduleGenerationRules => Set<ScheduleGenerationRule>();

    public DbSet<CalendarException> CalendarExceptions => Set<CalendarException>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ScheduleDbContext).Assembly);
    }
}
