namespace ScheduleService.DataAccess.Database.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScheduleService.Domain.Models;

public sealed class CalendarExceptionConfiguration : IEntityTypeConfiguration<CalendarException>
{
    public void Configure(EntityTypeBuilder<CalendarException> builder)
    {
        builder.ToTable("calendar_exceptions");

        builder.HasKey(calendarException => calendarException.Id);

        builder.Property(calendarException => calendarException.Id)
            .HasColumnName("id")
            .HasMaxLength(64)
            .ValueGeneratedNever();
        builder.Property(calendarException => calendarException.HolidayDate)
            .HasColumnName("holiday_date")
            .HasColumnType("date");
        builder.Property(calendarException => calendarException.TransferDate)
            .HasColumnName("transfer_date")
            .HasColumnType("date");

        builder.HasIndex(calendarException => calendarException.HolidayDate)
            .IsUnique()
            .HasDatabaseName("ux_calendar_exceptions_holiday_date");
        builder.HasIndex(calendarException => calendarException.TransferDate)
            .IsUnique()
            .HasFilter("transfer_date IS NOT NULL")
            .HasDatabaseName("ux_calendar_exceptions_transfer_date");
    }
}
