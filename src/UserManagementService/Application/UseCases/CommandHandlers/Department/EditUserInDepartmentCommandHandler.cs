﻿namespace UserManagementService.Application.UseCases.CommandHandlers.Department
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

        public EditUserInDepartmentCommandHandler(
            IUserRepository userRepository,
            IDepartmentRepository departmentRepository,
            IUserJobsRepository userJobsRepository)
        {
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

            var newUserJob = new UserJob()
            {
                Id = userJob.Id,
                UserId = userJob.UserId,
                DepartmentId = department.Id,
                Role = request.Role,
                Status = request.Status,
                PhoneNumber = request.PhoneNumber,
                Email = request.Email,
            };

            await userJobsRepository.UpdateAsync(newUserJob);

            return newUserJob;
        }
    }
}
