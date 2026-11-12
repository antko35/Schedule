namespace UserManagementService.PostgreSqlIntegration;

using Microsoft.EntityFrameworkCore;
using UserManagementService.DataAccess.Database;
using UserManagementService.DataAccess.Repository;
using UserManagementService.Domain.Models;
using Testcontainers.PostgreSql;
using Xunit;

public class PersistenceTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer database = new PostgreSqlBuilder().WithImage("postgres:17.6-alpine").Build();

    public async Task InitializeAsync()
    {
        await database.StartAsync();
        await using var db = Open();
        await db.Database.MigrateAsync();
    }

    public Task DisposeAsync() => database.DisposeAsync().AsTask();

    private UserManagementDbContext Open() => new(new DbContextOptionsBuilder<UserManagementDbContext>()
        .UseNpgsql(database.GetConnectionString(), options => options.MigrationsHistoryTable("__EFMigrationsHistory", UserManagementDbContext.SchemaName)).Options);

    [Fact]
    public async Task RepositoriesRoundTripDatesAgeContactsAndQueries()
    {
        string userId, departmentId;
        var birth = new DateOnly(1990, 2, 28);
        await using (var db = Open())
        {
            var user = new User { FirstName = "Anna", LastName = "A", DateOfBirth = birth };
            var other = new User { FirstName = "Boris", LastName = "B", DateOfBirth = birth };
            var empty = new User { DateOfBirth = birth };
            var department = new Department { ClinicId = EntityId.New(), DepartmentName = "Cardiology" };
            var clinic = new Clinic { ClinicName = "Clinic", ClinicDescription = "Description", Address = "Address" };
            var users = new UserRepository(db);
            await users.AddAsync(user);
            await users.AddAsync(other);
            await users.AddAsync(empty);
            await new DepartmentRepository(db).AddAsync(department);
            await new ClinicRepository(db).AddAsync(clinic);
            await new UserJobsRepository(db).AddAsync(new UserJob
            {
                UserId = user.Id, DepartmentId = department.Id, Role = UserJobRole.DepartmentHead,
                Email = "head@example.test", PhoneNumber = null,
            });
            Assert.Equal(0, await OpenCount()); // Staged writes are invisible until commit.
            await db.SaveChangesAsync();
            // Persist a deliberately stale age to verify materialization does not recalculate it.
            await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE user_management.users SET \"Age\" = {7} WHERE \"Id\" = {user.Id}");
            userId = user.Id; departmentId = department.Id;
        }

        await using (var db = Open())
        {
            await db.Database.MigrateAsync();
            var users = new UserRepository(db);
            var user = await users.GetByIdAsync(userId);
            Assert.Equal(birth, user.DateOfBirth);
            Assert.Equal(7, user.Age);
            var jobs = new UserJobsRepository(db);
            Assert.Null((await jobs.GetUserJobAsync(userId, departmentId)).PhoneNumber);
            Assert.Equal(new[] { "head@example.test" }, await jobs.GetHeadEmails(new[] { departmentId }));
            Assert.Empty(await jobs.GetHeadEmails(new[] { EntityId.New() }));
            var available = await users.GetAvailableUsersForDepartmentAsync(new[] { userId }, default);
            Assert.Equal(2, available.Count);
            Assert.DoesNotContain(available, item => item.Id == userId);
            Assert.Contains(available, item => item.FirstName == "" && item.LastName == "" && item.Patronymic == "");
            Assert.Single(await new DepartmentRepository(db).GetByClinicId((await db.Departments.SingleAsync()).ClinicId!));
            Assert.Equal("Address", (await new ClinicRepository(db).GetAllAsync()).Single().Address);
            Assert.Single(await users.GetByIdsAsync(new[] { userId }));
        }
    }

    private async Task<int> OpenCount()
    {
        await using var reader = Open();
        return await reader.Users.CountAsync();
    }

    [Fact]
    public async Task ConcurrentDuplicatesAreRejectedAndForeignKeysProtectMemberships()
    {
        var user = new User { DateOfBirth = new DateOnly(2000, 1, 1) };
        var department = new Department { ClinicId = EntityId.New(), DepartmentName = "D" };
        await using (var db = Open())
        {
            db.AddRange(user, department);
            await db.SaveChangesAsync();
        }

        async Task<bool> InsertJob()
        {
            await using var db = Open();
            db.UserJobs.Add(new UserJob { UserId = user.Id, DepartmentId = department.Id });
            try { await db.SaveChangesAsync(); return true; }
            catch (DbUpdateException exception) { Assert.Equal("23505", Assert.IsType<Npgsql.PostgresException>(exception.InnerException).SqlState); return false; }
        }
        var result = await Task.WhenAll(InsertJob(), InsertJob());
        Assert.Single(result.Where(success => success));

        async Task<bool> InsertDepartment()
        {
            await using var db = Open();
            db.Departments.Add(new Department { ClinicId = department.ClinicId, DepartmentName = "Concurrent" });
            try { await db.SaveChangesAsync(); return true; }
            catch (DbUpdateException exception) { Assert.Equal("23505", Assert.IsType<Npgsql.PostgresException>(exception.InnerException).SqlState); return false; }
        }
        Assert.Single((await Task.WhenAll(InsertDepartment(), InsertDepartment())).Where(success => success));
        await using (var db = Open())
        {
            db.UserJobs.Add(new UserJob { UserId = EntityId.New(), DepartmentId = department.Id });
            await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
        }
        await using (var db = Open())
        {
            db.Users.Remove(await db.Users.SingleAsync());
            await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
        }
    }

    [Fact]
    public async Task FailedMultiRowCommitRollsBackAndDoesNotChangeOtherSchemas()
    {
        var user = new User { DateOfBirth = new DateOnly(1990, 1, 1) };
        var department = new Department { ClinicId = EntityId.New(), DepartmentName = "D" };
        await using (var db = Open())
        {
            await db.Database.ExecuteSqlRawAsync("CREATE SCHEMA schedule; CREATE TABLE schedule.sentinel (id int primary key); INSERT INTO schedule.sentinel VALUES (42)");
            db.AddRange(user, department);
            db.UserJobs.Add(new UserJob { UserId = user.Id, DepartmentId = department.Id });
            await db.SaveChangesAsync();
        }
        await using (var db = Open())
        {
            db.UserJobs.RemoveRange(await db.UserJobs.ToListAsync());
            db.Users.Remove(await db.Users.SingleAsync());
            db.UserJobs.Add(new UserJob { UserId = EntityId.New(), DepartmentId = department.Id });
            await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
        }
        await using (var db = Open())
        {
            await db.Database.MigrateAsync();
            Assert.Single(await db.Users.ToListAsync());
            Assert.Single(await db.UserJobs.ToListAsync());
            Assert.Equal(42, await db.Database.SqlQueryRaw<int>("SELECT id AS \"Value\" FROM schedule.sentinel").SingleAsync());
        }
    }
}
