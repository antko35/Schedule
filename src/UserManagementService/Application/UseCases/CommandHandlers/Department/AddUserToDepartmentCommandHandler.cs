namespace UserManagementService.Application.UseCases.CommandHandlers.Department;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using UserManagementService.Application.UseCases.Commands.Department;
using UserManagementService.Domain.Abstractions.IRepository;
using UserManagementService.Domain.Models;

public class AddUserToDepartmentCommandHandler : IRequestHandler<AddUserToDepartmentCommand, UserJob>
{
    private readonly IUserRepository userRepository;
    private readonly IDepartmentRepository departmentRepository;
    private readonly IUserJobsRepository userJobsRepository;

    public AddUserToDepartmentCommandHandler(IUserRepository userRepository,
                                             IDepartmentRepository departmentRepository,
                                             IUserJobsRepository userJobsRepository)
    {
        this.userRepository = userRepository;
        this.departmentRepository = departmentRepository;
        this.userJobsRepository = userJobsRepository;
    }

    public async Task<UserJob> Handle(AddUserToDepartmentCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId)
            ?? throw new KeyNotFoundException("User doesnt exist");

        var department = await departmentRepository.GetByIdAsync(request.DepartmentId)
            ?? throw new KeyNotFoundException("Department doesnt exist");

        var userJob = await userJobsRepository.GetUserJobAsync(request.UserId, request.DepartmentId);

        if (userJob != null)
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

        return newUserJob;
    }
}
