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
        var user = await userRepository.GetByIdAsync(request.userJob.UserId);
        if (user == null)
        {
            throw new InvalidOperationException("User doesnt exist");
        }

        var department = await departmentRepository.GetByIdAsync(request.userJob.DepartmentId);
        if (department == null)
        {
            throw new InvalidOperationException("Department doesnt exist");
        }

        var userJob = new UserJob()
        {
            UserId = user.Id,
            DepartmentId = department.Id,
            Role = request.userJob.Role,
            Status = request.userJob.Status,
        };

        await userJobsRepository.AddAsync(userJob);

        return userJob;
    }
}
