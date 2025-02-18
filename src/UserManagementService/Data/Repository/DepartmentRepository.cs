namespace UserManagementService.DataAccess.Repository;

using MongoDB.Driver;
using UserManagementService.DataAccess.Database;
using UserManagementService.Domain.Abstractions.IRepository;
using UserManagementService.Domain.Models;

public class DepartmentRepository : GenericRepository<Department>, IDepartmentRepository
{
    public DepartmentRepository(DbContext context, BaseDbOptions options)
        : base(context, options.DepartmentsCollectionName)
    {
    }

    public async Task<IEnumerable<Department>> GetByClinicId(string clinicId)
    {
        var filter = Builders<Department>.Filter.Eq(x => x.ClinicId, clinicId);
        var departments = await dbSet.Find(filter).ToListAsync();
        return departments;
    }
}
