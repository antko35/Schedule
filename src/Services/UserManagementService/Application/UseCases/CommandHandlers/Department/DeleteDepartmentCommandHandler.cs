using MediatR;
using UserManagementService.Application.Extensions;
using UserManagementService.Application.UseCases.Commands.Department;
using UserManagementService.Domain.Abstractions.IRabbitMq;
using UserManagementService.Domain.Abstractions.IRepository;

namespace UserManagementService.Application.UseCases.CommandHandlers.Department;

public class DeleteDepartmentCommandHandler : IRequestHandler<DeleteDepartmentCommand>
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IUserJobsRepository _userJobsRepository;
    private readonly IUserEventPublisher _userEventPublisher;
    private readonly IDataChanges changes;


    public DeleteDepartmentCommandHandler(
        IDepartmentRepository departmentRepository,
        IUserJobsRepository userJobsRepository,
        IUserEventPublisher userEventPublisher, IDataChanges changes)
    {
        this.changes = changes;
        _departmentRepository = departmentRepository;
        _userJobsRepository = userJobsRepository;
        _userEventPublisher = userEventPublisher;
    }

    public async Task Handle(DeleteDepartmentCommand request, CancellationToken cancellationToken)
    {
        var department = await _departmentRepository.GetByIdAsync(request.DepartmentId);

        department.EnsureExists("Department not found");

        var userJobs = (await _userJobsRepository.GetUserJobsByDepartmentIdAsync(request.DepartmentId)).ToList();
        foreach (var userJob in userJobs)
        {
            await _userJobsRepository.RemoveAsync(userJob.Id);
        }

        await _departmentRepository.RemoveAsync(request.DepartmentId);
        await changes.SaveChangesAsync(cancellationToken);

        foreach (var userJob in userJobs)
        {
            await _userEventPublisher.PublishUserDeleted(userJob.UserId, department.Id);
        }
    }
}
