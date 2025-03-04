using UserManagementService.Domain.Abstractions.IRabbitMq;

namespace UserManagementService.Application.UseCases.CommandHandlers.Department;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using UserManagementService.Application.Extensions;
using UserManagementService.Application.UseCases.Commands.Department;
using UserManagementService.Domain.Abstractions.IRepository;
using UserManagementService.Domain.Models;

public class AddUserToDepartmentCommandHandler : IRequestHandler<AddUserToDepartmentCommand, UserJob>
{
    private readonly IUserRepository userRepository;
    private readonly IDepartmentRepository departmentRepository;
    private readonly IUserJobsRepository userJobsRepository;
    private readonly IUserCreatedPublisher userCreatedPublisher;


    public AddUserToDepartmentCommandHandler(IUserRepository userRepository,
                                             IDepartmentRepository departmentRepository,
                                             IUserJobsRepository userJobsRepository,
                                             IUserCreatedPublisher userCreatedPublisher)
    {
        this.userRepository = userRepository;
        this.departmentRepository = departmentRepository;
        this.userJobsRepository = userJobsRepository;
        this.userCreatedPublisher = userCreatedPublisher;

    }

    public async Task<UserJob> Handle(AddUserToDepartmentCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId);

        user.EnsureExists("User not found");

        var department = await departmentRepository.GetByIdAsync(request.DepartmentId);

        department.EnsureExists("Department not found");

        var userInDepartment = await userJobsRepository.GetUserJobAsync(request.UserId, request.DepartmentId);

        if (userInDepartment != null)
        {
            throw new InvalidOperationException($"User {user.Id} already in department {department.Id}");
        }

        var newUserJob = new UserJob()
        {
           UserId = request.UserId,
           DepartmentId = request.DepartmentId,
           Role = request.Role,
           Status = request.Status,
           Email = request.Email,
           PhoneNumber = request.PhoneNumber,
        };

        await userJobsRepository.AddAsync(newUserJob);

        await userCreatedPublisher.PublishUserCreated(newUserJob.UserId, newUserJob.DepartmentId);

        return newUserJob;
    }
}
