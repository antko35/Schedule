namespace ScheduleService.DataAccess.Database.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScheduleService.Domain.Models;

public sealed class MonthlyScheduleConfiguration : IEntityTypeConfiguration<MonthlySchedule>
{
    public void Configure(EntityTypeBuilder<MonthlySchedule> builder)
    {
        builder.ToTable(
            "monthly_schedules",
            tableBuilder => tableBuilder.HasCheckConstraint(
                "ck_monthly_schedules_period_first_day",
                "EXTRACT(DAY FROM period) = 1"));

        builder.HasKey(schedule => schedule.Id);

        builder.Property(schedule => schedule.Id)
            .HasColumnName("id")
            .HasMaxLength(64)
            .ValueGeneratedNever();
        builder.Property(schedule => schedule.UserId)
            .HasColumnName("user_id")
            .HasMaxLength(128)
            .IsRequired();
        builder.Property(schedule => schedule.DepartmentId)
            .HasColumnName("department_id")
            .HasMaxLength(128)
            .IsRequired();
        builder.Property(schedule => schedule.Period)
            .HasColumnName("period")
            .HasColumnType("date");
        builder.Property(schedule => schedule.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .HasColumnType("timestamp with time zone");
        builder.Property(schedule => schedule.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .HasColumnType("timestamp with time zone");

        builder.HasIndex(schedule => new { schedule.UserId, schedule.DepartmentId, schedule.Period })
            .IsUnique()
            .HasDatabaseName("ux_monthly_schedules_user_department_period");
        builder.HasIndex(schedule => new { schedule.DepartmentId, schedule.Period })
            .HasDatabaseName("ix_monthly_schedules_department_period");

        builder.HasMany(schedule => schedule.WorkShifts)
            .WithOne()
            .HasForeignKey(shift => shift.MonthlyScheduleId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(schedule => schedule.WorkShifts)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
