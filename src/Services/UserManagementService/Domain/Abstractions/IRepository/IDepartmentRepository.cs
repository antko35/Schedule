namespace UserManagementService.Domain.Abstractions.IRepository;

using System.Collections.Generic;
using System.Threading.Tasks;
using UserManagementService.Domain.Models;

public interface IDepartmentRepository : IGenericRepository<Department>
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="clinicId"></param>
    /// <returns>List of departments in Clinic</returns>
    Task<IEnumerable<Department>> GetByClinicId(string clinicId);
}
