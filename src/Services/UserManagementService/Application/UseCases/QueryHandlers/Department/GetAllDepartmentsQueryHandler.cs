namespace UserManagementService.Application.UseCases.QueryHandlers.Department;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using UserManagementService.Application.UseCases.Queries.Department;
using UserManagementService.Domain.Abstractions.IRepository;
using UserManagementService.Domain.Models;
public class GetAllDepartmentsQueryHandler : IRequestHandler<GetAllDepartmentsQuery, IEnumerable<Department>>
{
    private readonly IDepartmentRepository departmentsRepository;

    public GetAllDepartmentsQueryHandler(IDepartmentRepository departmentsRepository)
    {
        this.departmentsRepository = departmentsRepository;
    }

    public async Task<IEnumerable<Department>> Handle(GetAllDepartmentsQuery request, CancellationToken cancellationToken)
    {
        var departments = await departmentsRepository.GetAllAsync();

        return departments;
    }
}
