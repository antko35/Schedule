namespace ScheduleService.DataAccess.Database.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScheduleService.Domain.Models;

public sealed class ScheduleGenerationRuleConfiguration : IEntityTypeConfiguration<ScheduleGenerationRule>
{
    public void Configure(EntityTypeBuilder<ScheduleGenerationRule> builder)
    {
        builder.ToTable(
            "schedule_generation_rules",
            tableBuilder =>
            {
                tableBuilder.HasCheckConstraint(
                    "ck_schedule_generation_rules_period_first_day",
                    "EXTRACT(DAY FROM period) = 1");
                tableBuilder.HasCheckConstraint(
                    "ck_schedule_generation_rules_target_minutes_positive",
                    "target_minutes > 0");
                tableBuilder.HasCheckConstraint(
                    "ck_schedule_generation_rules_max_shift_minutes_positive",
                    "max_shift_minutes > 0");
                tableBuilder.HasCheckConstraint(
                    "ck_schedule_generation_rules_pattern_defined",
                    "pattern BETWEEN 0 AND 6");
            });

        builder.HasKey(rule => rule.Id);

        builder.Property(rule => rule.Id)
            .HasColumnName("id")
            .HasMaxLength(64)
            .ValueGeneratedNever();
        builder.Property(rule => rule.UserId)
            .HasColumnName("user_id")
            .HasMaxLength(128)
            .IsRequired();
        builder.Property(rule => rule.DepartmentId)
            .HasColumnName("department_id")
            .HasMaxLength(128)
            .IsRequired();
        builder.Property(rule => rule.Period)
            .HasColumnName("period")
            .HasColumnType("date");
        builder.Property(rule => rule.TargetMinutes)
            .HasColumnName("target_minutes");
        builder.Property(rule => rule.MaxShiftMinutes)
            .HasColumnName("max_shift_minutes");
        builder.Property(rule => rule.FirstShiftStart)
            .HasColumnName("first_shift_start")
            .HasColumnType("time without time zone");
        builder.Property(rule => rule.SecondShiftStart)
            .HasColumnName("second_shift_start")
            .HasColumnType("time without time zone");
        builder.Property(rule => rule.Pattern)
            .HasColumnName("pattern")
            .HasConversion<int>();

        builder.HasIndex(rule => new { rule.UserId, rule.DepartmentId, rule.Period })
            .IsUnique()
            .HasDatabaseName("ux_schedule_generation_rules_user_department_period");
        builder.HasIndex(rule => new { rule.DepartmentId, rule.Period })
            .HasDatabaseName("ix_schedule_generation_rules_department_period");
    }
}
