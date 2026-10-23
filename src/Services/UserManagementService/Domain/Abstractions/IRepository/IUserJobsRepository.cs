namespace UserManagementService.Domain.Abstractions.IRepository
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using UserManagementService.Domain.Models;

    public interface IUserJobsRepository : IGenericRepository<UserJob>
    {
        Task DeleteByUserId(string userId);

        /// <summary>
        /// Get UserJob by userId and departmentId
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="departmentId"></param>
        /// <returns>UserJob in this department</returns>
        Task<UserJob> GetUserJobAsync(string userId, string departmentId);

        /// <summary>
        /// Get userJobs by departmentId
        /// </summary>
        /// <param name="departmentId"></param>
        /// <returns>List of userJobs</returns>
        Task<IEnumerable<UserJob>> GetUserJobsByDepartmentIdAsync(string departmentId);

        /// <summary>
        /// Get userJobs by UserId
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>List of User`s UserJob </returns>
        Task<IEnumerable<UserJob>> GetUserJobsByUserIdAsync(string userId);

        Task<List<string>> GetHeadEmails(IEnumerable<string> requestDepartmentsIds);
    }
}