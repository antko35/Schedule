namespace ScheduleService.DataAccess.Database.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScheduleService.Domain.Models;

public sealed class WorkShiftConfiguration : IEntityTypeConfiguration<WorkShift>
{
    public void Configure(EntityTypeBuilder<WorkShift> builder)
    {
        builder.ToTable(
            "work_shifts",
            tableBuilder => tableBuilder.HasCheckConstraint(
                "ck_work_shifts_valid_interval",
                "ends_at_utc > starts_at_utc"));

        builder.HasKey(shift => shift.Id);

        builder.Property(shift => shift.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();
        builder.Property(shift => shift.MonthlyScheduleId)
            .HasColumnName("monthly_schedule_id")
            .HasMaxLength(64)
            .IsRequired();
        builder.Property(shift => shift.WorkDate)
            .HasColumnName("work_date")
            .HasColumnType("date");
        builder.Property(shift => shift.StartsAtUtc)
            .HasColumnName("starts_at_utc")
            .HasColumnType("timestamp with time zone");
        builder.Property(shift => shift.EndsAtUtc)
            .HasColumnName("ends_at_utc")
            .HasColumnType("timestamp with time zone");

        builder.HasIndex(shift => new { shift.MonthlyScheduleId, shift.WorkDate })
            .IsUnique()
            .HasDatabaseName("ux_work_shifts_schedule_date");
    }
}
