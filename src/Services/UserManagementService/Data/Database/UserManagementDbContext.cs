namespace UserManagementService.DataAccess.Database;

using Microsoft.EntityFrameworkCore;
using UserManagementService.Domain.Abstractions.IRepository;
using UserManagementService.Domain.Models;

public class UserManagementDbContext(DbContextOptions<UserManagementDbContext> options)
    : DbContext(options), IDataChanges
{
    public const string SchemaName = "user_management";

    public DbSet<User> Users => Set<User>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<UserJob> UserJobs => Set<UserJob>();
    public DbSet<Clinic> Clinics => Set<Clinic>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserManagementDbContext).Assembly);
    }
}
