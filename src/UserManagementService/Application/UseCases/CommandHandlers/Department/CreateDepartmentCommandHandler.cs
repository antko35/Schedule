namespace UserManagementService.Application.UseCases.CommandHandlers.Department;

using MediatR;
using MongoDB.Bson;
using System.Threading;
using System.Threading.Tasks;
using UserManagementService.Application.UseCases.Commands.Department;
using UserManagementService.Domain.Abstractions.IRepository;
using UserManagementService.Domain.Models;
public class CreateDepartmentCommandHandler
    : IRequestHandler<CreateDepartmentCommand, Department>
{
    private readonly IDepartmentRepository departmentRepository;

    public CreateDepartmentCommandHandler(IDepartmentRepository departmentRepository)
    {
        this.departmentRepository = departmentRepository;
    }

    public async Task<Department> Handle(CreateDepartmentCommand request, CancellationToken cancellationToken)
    {
        var clinicId = request.ClinicId;
        if (String.IsNullOrEmpty(clinicId))
        {
            clinicId = ObjectId.GenerateNewId().ToString();
        }

        var departmentsInClinic = await departmentRepository.GetByClinicId(clinicId);

        var departmentToCreate = new Department()
        {
            ClinicId = clinicId,
            DepartmentName = request.DeartmentName,
        };

        if (departmentsInClinic.Any(x => x.ClinicId == departmentToCreate.ClinicId && x.DepartmentName == departmentToCreate.DepartmentName))
        {
            throw new InvalidOperationException("This department already exist");
        }

        await departmentRepository.AddAsync(departmentToCreate);

        return departmentToCreate;
    }
}
