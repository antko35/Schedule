namespace UserManagementService.Application.UseCases.CommandHandlers.Department;

using MediatR;
using System.Threading;
using System.Threading.Tasks;
using UserManagementService.Application.UseCases.Commands.Department;
using UserManagementService.Domain.Abstractions.IRepository;
using UserManagementService.Domain.Models;
public class CreateDepartmentCommandHandler
    : IRequestHandler<CreateDepartmentCommand, Department>
{
    private readonly IDepartmentRepository departmentRepository;
    private readonly IDataChanges changes;


    public CreateDepartmentCommandHandler(IDepartmentRepository departmentRepository, IDataChanges changes)
    {
        this.changes = changes;
        this.departmentRepository = departmentRepository;
    }

    public async Task<Department> Handle(CreateDepartmentCommand request, CancellationToken cancellationToken)
    {
        var clinicId = request.ClinicId;
        if (String.IsNullOrEmpty(clinicId))
        {
            clinicId = EntityId.New();
        }

        var departmentsInClinic = await departmentRepository.GetByClinicId(EntityId.Normalize(clinicId));

        var departmentToCreate = new Department()
        {
            ClinicId = EntityId.Normalize(clinicId),
            DepartmentName = request.DeartmentName,
        };

        if (departmentsInClinic.Any(x => x.ClinicId == departmentToCreate.ClinicId && x.DepartmentName == departmentToCreate.DepartmentName))
        {
            throw new InvalidOperationException("This department already exist");
        }

        await departmentRepository.AddAsync(departmentToCreate);
        await changes.SaveChangesAsync(cancellationToken);

        return departmentToCreate;
    }
}
