namespace UserManagementService.Application.UseCases.QueryHandlers.Department;

using MediatR;
using UserManagementService.Application.UseCases.Queries.Department;
using UserManagementService.Domain.Abstractions.IRepository;
using UserManagementService.Domain.Models;
public class GetDepartmentByIdQueryHandler : IRequestHandler<GetDepartmentByIdQuery, Department>
{
    private readonly IDepartmentRepository departmentRepository;

    public GetDepartmentByIdQueryHandler(IDepartmentRepository departmentRepository)
    {
        this.departmentRepository = departmentRepository;
    }

    public async Task<Department> Handle(GetDepartmentByIdQuery request, CancellationToken cancellationToken)
    {
        var department = await departmentRepository.GetByIdAsync(request.id)
            ?? throw new KeyNotFoundException("Department not found");
        return department;
    }
}
