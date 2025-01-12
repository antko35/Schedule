namespace UserManagementService.DataAccess.Repository;

using UserManagementService.DataAccess.Database;
using UserManagementService.Domain.Abstractions.IRepository;
using UserManagementService.Domain.Models;

public class DepartmentRepository : GenericRepository<Department>, IDepartmentRepository
{
    public DepartmentRepository(DbContext context, IDbOptions options)
        : base(context, options.DepartmentsCollectionName)
    {
    }
}
