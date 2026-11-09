namespace UserManagementService.DataAccess.Repository;

using Microsoft.EntityFrameworkCore;
using UserManagementService.DataAccess.Database;
using UserManagementService.Domain.Abstractions.IRepository;
using UserManagementService.Domain.Models;

public class DepartmentRepository(UserManagementDbContext context)
    : GenericRepository<Department>(context), IDepartmentRepository
{
    public async Task<IEnumerable<Department>> GetByClinicId(string clinicId)
    {
        clinicId = EntityId.Normalize(clinicId);
        return await dbSet.Where(entity => entity.ClinicId == clinicId).ToListAsync();
    }
}
