namespace UserManagementService.DataAccess.Repository;

using UserManagementService.DataAccess.Database;
using UserManagementService.Domain.Abstractions.IRepository;
using UserManagementService.Domain.Models;
public class UserJobsRepository : GenericRepository<UserJob>, IUserJobsRepository
{
    public UserJobsRepository(DbContext dbContext, IDbOptions options)
        : base(dbContext, options.UserJobsCollectionName)
    {
    }
}
