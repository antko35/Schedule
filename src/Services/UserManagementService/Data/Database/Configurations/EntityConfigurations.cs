namespace UserManagementService.DataAccess.Database.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserManagementService.Domain.Models;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Id).HasMaxLength(24).ValueGeneratedNever();
        builder.Property(entity => entity.DateOfBirth).HasColumnType("date")
            .HasField("dateOfBirth").UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Property(entity => entity.Age);
    }
}

public sealed class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("departments");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Id).HasMaxLength(24).ValueGeneratedNever();
        builder.Property(entity => entity.ClinicId).HasMaxLength(24);
        builder.HasIndex(entity => new { entity.ClinicId, entity.DepartmentName }).IsUnique()
            .HasDatabaseName("ux_departments_clinic_name");
    }
}

public sealed class UserJobConfiguration : IEntityTypeConfiguration<UserJob>
{
    public void Configure(EntityTypeBuilder<UserJob> builder)
    {
        builder.ToTable("user_jobs");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Id).HasMaxLength(24).ValueGeneratedNever();
        builder.Property(entity => entity.UserId).HasMaxLength(24).IsRequired();
        builder.Property(entity => entity.DepartmentId).HasMaxLength(24).IsRequired();
        builder.HasOne<User>().WithMany().HasForeignKey(entity => entity.UserId)
            .OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_user_jobs_users");
        builder.HasOne<Department>().WithMany().HasForeignKey(entity => entity.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_user_jobs_departments");
        builder.HasIndex(entity => new { entity.UserId, entity.DepartmentId }).IsUnique()
            .HasDatabaseName("ux_user_jobs_user_department");
        builder.HasIndex(entity => entity.DepartmentId);
    }
}

public sealed class ClinicConfiguration : IEntityTypeConfiguration<Clinic>
{
    public void Configure(EntityTypeBuilder<Clinic> builder)
    {
        builder.ToTable("clinics");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Id).HasMaxLength(24).ValueGeneratedNever();
    }
}
