namespace UserManagementService.Application.UseCases.CommandHandlers.Department
{
    using System.Reflection.Metadata;
    using MediatR;
    using UserManagementService.Application.DTOs;
    using UserManagementService.Application.Extensions;
    using UserManagementService.Application.UseCases.Commands.Department;
    using UserManagementService.Domain.Abstractions.IRepository;
    using UserManagementService.Domain.Models;

    public class EditUserInDepartmentCommandHandler
        : IRequestHandler<EditUserInDepartmentCommand, UserJob>
    {
        private readonly IUserJobsRepository userJobsRepository;
        private readonly IUserRepository userRepository;
        private readonly IDepartmentRepository departmentRepository;
        private readonly IDataChanges changes;


        public EditUserInDepartmentCommandHandler(
            IUserRepository userRepository,
            IDepartmentRepository departmentRepository,
            IUserJobsRepository userJobsRepository, IDataChanges changes)
        {
            this.changes = changes;
            this.userRepository = userRepository;
            this.departmentRepository = departmentRepository;
            this.userJobsRepository = userJobsRepository;
        }

        public async Task<UserJob> Handle(EditUserInDepartmentCommand request, CancellationToken cancellationToken)
        {
            var user = await userRepository.GetByIdAsync(request.UserId);

            user.EnsureExists("User not found");

            var department = await departmentRepository.GetByIdAsync(request.DepartmentId);

            department.EnsureExists("Department not found");

            var userJob = await userJobsRepository.GetUserJobAsync(request.UserId, request.DepartmentId);

            userJob.EnsureExists("User not found in this department");

            userJob.Role = request.Role;
            userJob.Status = request.Status;
            userJob.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber;
            userJob.Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email;
            await userJobsRepository.UpdateAsync(userJob);
            await changes.SaveChangesAsync(cancellationToken);
            return userJob;
        }
    }
}
